namespace Plugin.Maui.PushRouter;

/// <summary>
/// Outcome of a single notification dispatch.
/// </summary>
public sealed class PushDispatchResult
{
	PushDispatchResult(
		PushNotification notification,
		bool queued,
		bool duplicate,
		bool handled,
		bool navigated,
		string? routeKey,
		string? navigationRoute,
		PushRouteResult? handlerResult)
	{
		Notification = notification;
		Queued = queued;
		Duplicate = duplicate;
		Handled = handled;
		Navigated = navigated;
		RouteKey = routeKey;
		NavigationRoute = navigationRoute;
		HandlerResult = handlerResult;
	}

	public PushNotification Notification { get; }

	public bool Queued { get; }

	public bool Duplicate { get; }

	public bool Handled { get; }

	public bool Navigated { get; }

	public string? RouteKey { get; }

	public string? NavigationRoute { get; }

	public PushRouteResult? HandlerResult { get; }

	internal static PushDispatchResult ForQueued(PushNotification notification) =>
		new(notification, queued: true, duplicate: false, handled: false, navigated: false, null, null, null);

	internal static PushDispatchResult ForDuplicate(PushNotification notification) =>
		new(notification, queued: false, duplicate: true, handled: true, navigated: false, null, null, null);

	internal static PushDispatchResult ForDispatch(
		PushNotification notification,
		bool handled,
		bool navigated,
		string? routeKey,
		string? navigationRoute,
		PushRouteResult? handlerResult) =>
		new(notification, queued: false, duplicate: false, handled, navigated, routeKey, navigationRoute, handlerResult);
}
