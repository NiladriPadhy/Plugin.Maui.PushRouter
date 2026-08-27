namespace Plugin.Maui.PushRouter;

/// <summary>
/// Arguments passed to an <see cref="IPushRouteHandler"/>.
/// </summary>
public sealed class PushRouteContext
{
	public PushRouteContext(
		PushNotification notification,
		string routeKey,
		PushRouterOptions options,
		IServiceProvider? services)
	{
		Notification = notification ?? throw new ArgumentNullException(nameof(notification));
		RouteKey = routeKey ?? throw new ArgumentNullException(nameof(routeKey));
		Options = options ?? throw new ArgumentNullException(nameof(options));
		Services = services;
	}

	public PushNotification Notification { get; }

	/// <summary>
	/// Gets the resolved handler / map key (for example <c>order</c>).
	/// </summary>
	public string RouteKey { get; }

	public PushRouterOptions Options { get; }

	/// <summary>
	/// Gets the MAUI service provider when the plugin was registered with <c>UsePushRouter</c>.
	/// </summary>
	public IServiceProvider? Services { get; }
}
