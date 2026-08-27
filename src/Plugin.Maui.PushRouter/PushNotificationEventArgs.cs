namespace Plugin.Maui.PushRouter;

/// <summary>
/// Raised when a notification is received or tapped.
/// </summary>
public sealed class PushNotificationEventArgs : EventArgs
{
	public PushNotificationEventArgs(PushNotification notification)
	{
		Notification = notification ?? throw new ArgumentNullException(nameof(notification));
	}

	public PushNotification Notification { get; }
}
