namespace Plugin.Maui.PushRouter;

/// <summary>
/// Routes FCM / APNs notifications to registered handlers and MAUI screens.
/// </summary>
public interface IPushRouter
{
	/// <summary>
	/// Gets a value indicating whether routing is available on this target.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	/// Gets the native transport used on this platform.
	/// </summary>
	PushRouterPlatformInfo Platform { get; }

	/// <summary>
	/// Gets the options the router was created with.
	/// </summary>
	PushRouterOptions Options { get; }

	/// <summary>
	/// Gets a value indicating whether queued cold-start notifications may be flushed.
	/// </summary>
	bool IsReady { get; }

	/// <summary>
	/// Raised for a foreground / data-message receive.
	/// </summary>
	event EventHandler<PushNotificationEventArgs>? Received;

	/// <summary>
	/// Raised when the user opens the app from a notification.
	/// </summary>
	event EventHandler<PushNotificationEventArgs>? Tapped;

	/// <summary>
	/// Raised after a notification was handled and/or navigated.
	/// </summary>
	event EventHandler<PushRoutedEventArgs>? Routed;

	/// <summary>
	/// Raised when a tap has no handler and no navigable route.
	/// </summary>
	event EventHandler<PushUnhandledEventArgs>? Unhandled;

	/// <summary>
	/// Raised when a handler or navigator throws.
	/// </summary>
	event EventHandler<PushFailedEventArgs>? Failed;

	/// <summary>
	/// Maps a payload key to a Shell path. Tokens such as <c>{orderId}</c> are replaced from the payload.
	/// </summary>
	IPushRouter Map(string key, string routeTemplate);

	/// <summary>
	/// Maps a payload key to a Shell path produced from the notification.
	/// </summary>
	IPushRouter Map(string key, Func<PushNotification, string> routeFactory);

	/// <summary>
	/// Registers a handler for a payload key.
	/// </summary>
	IPushRouter Handle(string key, IPushRouteHandler handler);

	/// <summary>
	/// Registers a delegate handler for a payload key.
	/// </summary>
	IPushRouter Handle(string key, Func<PushRouteContext, CancellationToken, Task<PushRouteResult>> handler);

	/// <summary>
	/// Registers a delegate handler that returns <see cref="PushRouteResult.Handled"/>.
	/// </summary>
	IPushRouter Handle(string key, Action<PushRouteContext> handler);

	/// <summary>
	/// Removes a handler and route map for <paramref name="key"/>.
	/// </summary>
	bool Unregister(string key);

	/// <summary>
	/// Parses and routes a foreground / data-message payload.
	/// </summary>
	Task<PushDispatchResult> HandleReceivedAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default);

	/// <summary>
	/// Parses and routes a notification-tap / cold-start payload.
	/// </summary>
	Task<PushDispatchResult> HandleTappedAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default);

	/// <summary>
	/// Routes an already-parsed notification.
	/// </summary>
	Task<PushDispatchResult> HandleAsync(PushNotification notification, CancellationToken cancellationToken = default);

	/// <summary>
	/// Fire-and-forget receive. Exceptions are raised on <see cref="Failed"/>.
	/// </summary>
	void HandleReceived(IReadOnlyDictionary<string, string> data);

	/// <summary>
	/// Fire-and-forget tap. Exceptions are raised on <see cref="Failed"/>.
	/// </summary>
	void HandleTapped(IReadOnlyDictionary<string, string> data);

	/// <summary>
	/// Fire-and-forget dispatch of a parsed notification.
	/// </summary>
	void Handle(PushNotification notification);

	/// <summary>
	/// Marks the app ready (Shell created) and flushes queued cold-start notifications.
	/// </summary>
	void MarkReady();

	/// <summary>
	/// Enables or disables plugin diagnostics.
	/// </summary>
	void EnableLogging(bool enabled, IPushRouterLogger? logger = null);
}
