namespace Plugin.Maui.PushRouter;

/// <summary>
/// Raised when a handler or navigator throws.
/// </summary>
public sealed class PushFailedEventArgs : EventArgs
{
	public PushFailedEventArgs(PushNotification notification, Exception exception)
	{
		Notification = notification ?? throw new ArgumentNullException(nameof(notification));
		Exception = exception ?? throw new ArgumentNullException(nameof(exception));
	}

	public PushNotification Notification { get; }

	public Exception Exception { get; }
}
