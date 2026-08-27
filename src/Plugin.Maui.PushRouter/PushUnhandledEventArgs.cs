namespace Plugin.Maui.PushRouter;

/// <summary>
/// Raised when a tapped notification has no handler and no navigable route.
/// </summary>
public sealed class PushUnhandledEventArgs : EventArgs
{
	public PushUnhandledEventArgs(PushNotification notification, string? routeKey)
	{
		Notification = notification ?? throw new ArgumentNullException(nameof(notification));
		RouteKey = routeKey;
	}

	public PushNotification Notification { get; }

	public string? RouteKey { get; }
}
