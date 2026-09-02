namespace Plugin.Maui.PushRouter;

/// <summary>
/// Shared configuration applied when the plugin is registered with <c>UsePushRouter</c>.
/// </summary>
public sealed class PushRouterOptions
{
	/// <summary>
	/// Gets or sets the payload key that holds the destination (handler name or Shell path). Default is <c>route</c>.
	/// </summary>
	public string RouteKey { get; set; } = "route";

	/// <summary>
	/// Gets or sets the payload key used when <see cref="RouteKey"/> is missing. Default is <c>type</c>.
	/// </summary>
	public string TypeKey { get; set; } = "type";

	/// <summary>
	/// Gets or sets the payload key for the notification title. Default is <c>title</c>.
	/// </summary>
	public string? TitleKey { get; set; } = "title";

	/// <summary>
	/// Gets or sets the payload key for the notification body. Default is <c>body</c>.
	/// </summary>
	public string? BodyKey { get; set; } = "body";

	/// <summary>
	/// Gets or sets an explicit message-id key. When <c>null</c>, FCM/APNs ids are auto-detected.
	/// </summary>
	public string? MessageIdKey { get; set; }

	/// <summary>
	/// Gets or sets a fallback handler/map key when the payload has no route or type.
	/// </summary>
	public string? DefaultRouteKey { get; set; }

	/// <summary>
	/// Gets or sets a Shell path used when a key is resolved but has no map (for example <c>//inbox</c>).
	/// </summary>
	public string? DefaultRoute { get; set; }

	/// <summary>
	/// When <c>true</c>, a payload value that looks like a Shell path is used even if it is not
	/// in <see cref="Map(string, string)"/>. Default is <c>false</c> — only registered maps and
	/// <see cref="DefaultRoute"/> may navigate.
	/// </summary>
	public bool AllowUnmappedPayloadRoutes { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether mapped routes are opened with <see cref="IPushNavigator"/>.
	/// Default is <c>true</c>.
	/// </summary>
	public bool EnableShellNavigation { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether Shell navigation runs only for taps / cold starts.
	/// Foreground receives still invoke handlers. Default is <c>true</c>.
	/// </summary>
	public bool NavigateOnTapOnly { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether notifications arriving before <c>MarkReady</c> are queued.
	/// Default is <c>true</c>.
	/// </summary>
	public bool QueueUntilReady { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether the same <see cref="PushNotification.MessageId"/> is processed only once.
	/// Default is <c>true</c>.
	/// </summary>
	public bool Deduplicate { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether plugin logging starts enabled.
	/// </summary>
	public bool EnableLogging { get; set; }

	/// <summary>
	/// Gets or sets a custom logger. When <c>null</c>, the plugin uses Microsoft.Extensions.Logging if available, otherwise a debug logger.
	/// </summary>
	public IPushRouterLogger? Logger { get; set; }

	/// <summary>
	/// Gets or sets a custom navigator. When <c>null</c>, <see cref="ShellPushNavigator"/> is used.
	/// </summary>
	public IPushNavigator? Navigator { get; set; }

	internal List<RouteMapRegistration> RouteMaps { get; } = [];

	internal List<HandlerRegistration> Handlers { get; } = [];

	/// <summary>
	/// Maps a payload key to a Shell path. Tokens such as <c>{orderId}</c> are replaced from the payload.
	/// </summary>
	/// <example>
	/// <code>
	/// options.Map("order", "//order?id={orderId}");
	/// </code>
	/// </example>
	public PushRouterOptions Map(string key, string routeTemplate)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentException.ThrowIfNullOrWhiteSpace(routeTemplate);
		RouteMaps.Add(new RouteMapRegistration(key, routeTemplate, null));
		return this;
	}

	/// <summary>
	/// Maps a payload key to a Shell path produced from the notification.
	/// </summary>
	public PushRouterOptions Map(string key, Func<PushNotification, string> routeFactory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentNullException.ThrowIfNull(routeFactory);
		RouteMaps.Add(new RouteMapRegistration(key, null, routeFactory));
		return this;
	}

	/// <summary>
	/// Registers a handler for a payload key.
	/// </summary>
	public PushRouterOptions Handle(string key, IPushRouteHandler handler)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ArgumentNullException.ThrowIfNull(handler);
		Handlers.Add(new HandlerRegistration(key, handler));
		return this;
	}

	/// <summary>
	/// Registers a delegate handler for a payload key.
	/// </summary>
	public PushRouterOptions Handle(string key, Func<PushRouteContext, CancellationToken, Task<PushRouteResult>> handler)
	{
		return Handle(key, new DelegatePushRouteHandler(handler));
	}

	/// <summary>
	/// Registers a delegate handler that always returns <see cref="PushRouteResult.Handled"/>.
	/// </summary>
	public PushRouterOptions Handle(string key, Action<PushRouteContext> handler)
	{
		return Handle(key, new DelegatePushRouteHandler(handler));
	}
}

sealed record RouteMapRegistration(string Key, string? Template, Func<PushNotification, string>? Factory);

sealed record HandlerRegistration(string Key, IPushRouteHandler Handler);
