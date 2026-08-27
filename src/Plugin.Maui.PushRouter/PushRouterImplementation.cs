namespace Plugin.Maui.PushRouter;

sealed class PushRouterImplementation : IPushRouter
{
	readonly object _gate = new();
	readonly Dictionary<string, RouteMapRegistration> _routes = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, IPushRouteHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);
	readonly Queue<PushNotification> _queue = new();
	readonly ProcessedIdCache _processed = new(64);
	readonly IPushNavigator _navigator;
	IPushRouterLogger? _logger;
	bool _logging;
	bool _ready;

	internal PushRouterImplementation(PushRouterOptions options, IServiceProvider? services = null)
	{
		Options = options ?? throw new ArgumentNullException(nameof(options));
		Services = services;
		_navigator = options.Navigator ?? new ShellPushNavigator();
		_logging = options.EnableLogging;
		_logger = options.Logger;

		foreach (var map in options.RouteMaps)
			_routes[map.Key] = map;

		foreach (var handler in options.Handlers)
			_handlers[handler.Key] = handler.Handler;
	}

	internal IServiceProvider? Services { get; set; }

	public bool IsSupported => Platform.IsSupported;

	public PushRouterPlatformInfo Platform { get; } = PushRouterPlatformInfo.Current;

	public PushRouterOptions Options { get; }

	public bool IsReady
	{
		get
		{
			lock (_gate)
				return _ready;
		}
	}

	public event EventHandler<PushNotificationEventArgs>? Received;
	public event EventHandler<PushNotificationEventArgs>? Tapped;
	public event EventHandler<PushRoutedEventArgs>? Routed;
	public event EventHandler<PushUnhandledEventArgs>? Unhandled;
	public event EventHandler<PushFailedEventArgs>? Failed;

	public IPushRouter Map(string key, string routeTemplate)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentException.ThrowIfNullOrWhiteSpace(routeTemplate);
		lock (_gate)
			_routes[key] = new RouteMapRegistration(key, routeTemplate, null);
		return this;
	}

	public IPushRouter Map(string key, Func<PushNotification, string> routeFactory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentNullException.ThrowIfNull(routeFactory);
		lock (_gate)
			_routes[key] = new RouteMapRegistration(key, null, routeFactory);
		return this;
	}

	public IPushRouter Handle(string key, IPushRouteHandler handler)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentNullException.ThrowIfNull(handler);
		lock (_gate)
			_handlers[key] = handler;
		return this;
	}

	public IPushRouter Handle(string key, Func<PushRouteContext, CancellationToken, Task<PushRouteResult>> handler) =>
		Handle(key, new DelegatePushRouteHandler(handler));

	public IPushRouter Handle(string key, Action<PushRouteContext> handler) =>
		Handle(key, new DelegatePushRouteHandler(handler));

	public bool Unregister(string key)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		lock (_gate)
		{
			var removedHandler = _handlers.Remove(key);
			var removedMap = _routes.Remove(key);
			return removedHandler || removedMap;
		}
	}

	public Task<PushDispatchResult> HandleReceivedAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default) =>
		HandleAsync(PushNotificationParser.Parse(data, Options, PushDelivery.Received), cancellationToken);

	public Task<PushDispatchResult> HandleTappedAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default) =>
		HandleAsync(PushNotificationParser.Parse(data, Options, PushDelivery.Tapped), cancellationToken);

	public async Task<PushDispatchResult> HandleAsync(PushNotification notification, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(notification);

		if (Options.Deduplicate && !string.IsNullOrWhiteSpace(notification.MessageId))
		{
			lock (_gate)
			{
				if (!_processed.TryAdd(notification.MessageId))
				{
					Log(PushRouterLogLevel.Debug, $"Ignored duplicate {notification}");
					return PushDispatchResult.ForDuplicate(notification);
				}
			}
		}

		RaiseDelivery(notification);

		var shouldQueue = Options.QueueUntilReady && !IsReady;
		if (shouldQueue)
		{
			lock (_gate)
				_queue.Enqueue(notification);

			Log(PushRouterLogLevel.Debug, $"Queued {notification}");
			return PushDispatchResult.ForQueued(notification);
		}

		return await DispatchCoreAsync(notification, cancellationToken).ConfigureAwait(false);
	}

	public void HandleReceived(IReadOnlyDictionary<string, string> data) =>
		FireAndForget(() => HandleReceivedAsync(data));

	public void HandleTapped(IReadOnlyDictionary<string, string> data) =>
		FireAndForget(() => HandleTappedAsync(data));

	public void Handle(PushNotification notification) =>
		FireAndForget(() => HandleAsync(notification));

	public void MarkReady()
	{
		List<PushNotification> pending;
		lock (_gate)
		{
			_ready = true;
			pending = [.. _queue];
			_queue.Clear();
		}

		Log(PushRouterLogLevel.Information, $"Ready. Flushing {pending.Count} queued notification(s).");

		foreach (var notification in pending)
			FireAndForget(() => DispatchCoreAsync(notification, CancellationToken.None));
	}

	public void EnableLogging(bool enabled, IPushRouterLogger? logger = null)
	{
		_logging = enabled;
		if (logger is not null)
			_logger = logger;
		else if (enabled)
			_logger ??= new DebugPushRouterLogger();
	}

	async Task<PushDispatchResult> DispatchCoreAsync(PushNotification notification, CancellationToken cancellationToken)
	{
		var routeKey = RouteKeyResolver.Resolve(notification, Options);
		IPushRouteHandler? handler = null;
		RouteMapRegistration? map = null;

		if (!string.IsNullOrWhiteSpace(routeKey))
		{
			lock (_gate)
			{
				_handlers.TryGetValue(routeKey, out handler);
				_routes.TryGetValue(routeKey, out map);
			}
		}

		PushRouteResult? handlerResult = null;
		if (handler is not null)
		{
			try
			{
				var context = new PushRouteContext(notification, routeKey!, Options, Services);
				handlerResult = await handler.HandleAsync(context, cancellationToken).ConfigureAwait(false);
				Log(PushRouterLogLevel.Debug, $"Handler '{routeKey}' returned {handlerResult}.");
			}
			catch (Exception ex)
			{
				Log(PushRouterLogLevel.Error, $"Handler '{routeKey}' failed.", ex);
				Failed?.Invoke(this, new PushFailedEventArgs(notification, ex));
				return PushDispatchResult.ForDispatch(notification, handled: false, navigated: false, routeKey, null, null);
			}
		}

		if (handlerResult is PushRouteResult.Ignore)
			return PushDispatchResult.ForDispatch(notification, handled: true, navigated: false, routeKey, null, handlerResult);

		var wantsNavigation = ShouldNavigate(notification, handlerResult);
		string? navigationRoute = null;

		if (wantsNavigation)
		{
			navigationRoute = ResolveNavigationRoute(notification, routeKey, map);
			if (!string.IsNullOrWhiteSpace(navigationRoute))
			{
				try
				{
					if (!_navigator.CanNavigate)
					{
						throw new PushRouterException(
							PushRouterError.NavigationUnavailable,
							"The navigator is not ready. Call MarkReady() after Shell is created.");
					}

					await _navigator.NavigateAsync(navigationRoute, notification, cancellationToken).ConfigureAwait(false);
					var result = PushDispatchResult.ForDispatch(notification, handled: true, navigated: true, routeKey, navigationRoute, handlerResult);
					Routed?.Invoke(this, new PushRoutedEventArgs(result));
					Log(PushRouterLogLevel.Information, $"Navigated {notification} -> {navigationRoute}");
					return result;
				}
				catch (Exception ex)
				{
					Log(PushRouterLogLevel.Error, $"Navigation to '{navigationRoute}' failed.", ex);
					Failed?.Invoke(this, new PushFailedEventArgs(notification, ex));
					return PushDispatchResult.ForDispatch(notification, handled: handlerResult is PushRouteResult.Handled, navigated: false, routeKey, navigationRoute, handlerResult);
				}
			}
		}

		var handled = handlerResult is PushRouteResult.Handled or PushRouteResult.Navigate
			|| (notification.Delivery == PushDelivery.Received && Options.NavigateOnTapOnly);

		if (!handled && notification.Delivery == PushDelivery.Tapped)
		{
			Unhandled?.Invoke(this, new PushUnhandledEventArgs(notification, routeKey));
			Log(PushRouterLogLevel.Warning, $"Unhandled tap {notification} key={routeKey ?? "(none)"}");
		}
		else if (handled)
		{
			var result = PushDispatchResult.ForDispatch(notification, handled: true, navigated: false, routeKey, navigationRoute, handlerResult);
			Routed?.Invoke(this, new PushRoutedEventArgs(result));
			return result;
		}

		return PushDispatchResult.ForDispatch(notification, handled, navigated: false, routeKey, navigationRoute, handlerResult);
	}

	bool ShouldNavigate(PushNotification notification, PushRouteResult? handlerResult)
	{
		if (!Options.EnableShellNavigation)
			return false;

		if (handlerResult is PushRouteResult.Handled or PushRouteResult.Ignore)
			return false;

		if (handlerResult is PushRouteResult.Navigate)
			return true;

		if (Options.NavigateOnTapOnly && notification.Delivery != PushDelivery.Tapped)
			return false;

		return true;
	}

	string? ResolveNavigationRoute(PushNotification notification, string? routeKey, RouteMapRegistration? map)
	{
		if (map?.Factory is not null)
		{
			var produced = map.Factory(notification);
			return string.IsNullOrWhiteSpace(produced) ? null : produced;
		}

		if (!string.IsNullOrWhiteSpace(map?.Template))
			return RouteTemplate.Expand(map.Template, notification.Data);

		var raw = notification.Route ?? PayloadLookup.Get(notification.Data, Options.RouteKey);
		if (!string.IsNullOrWhiteSpace(raw) && RouteKeyResolver.IsPath(raw))
			return raw;

		if (!string.IsNullOrWhiteSpace(Options.DefaultRoute))
			return RouteTemplate.Expand(Options.DefaultRoute, notification.Data);

		if (!string.IsNullOrWhiteSpace(routeKey) && RouteKeyResolver.IsPath(routeKey))
			return routeKey;

		return null;
	}

	void RaiseDelivery(PushNotification notification)
	{
		var args = new PushNotificationEventArgs(notification);
		if (notification.Delivery == PushDelivery.Tapped)
			Tapped?.Invoke(this, args);
		else
			Received?.Invoke(this, args);
	}

	void FireAndForget(Func<Task> work)
	{
		_ = RunSafe(work);
	}

	async Task RunSafe(Func<Task> work)
	{
		try
		{
			await work().ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Log(PushRouterLogLevel.Error, "Unhandled dispatch failure.", ex);
		}
	}

	void Log(PushRouterLogLevel level, string message, Exception? exception = null)
	{
		if (!_logging)
			return;

		(_logger ?? new DebugPushRouterLogger()).Log(level, message, exception);
	}
}
