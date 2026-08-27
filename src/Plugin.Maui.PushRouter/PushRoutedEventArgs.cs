namespace Plugin.Maui.PushRouter;

/// <summary>
/// Raised after a notification was handled and/or navigated.
/// </summary>
public sealed class PushRoutedEventArgs : EventArgs
{
	public PushRoutedEventArgs(PushDispatchResult result)
	{
		Result = result ?? throw new ArgumentNullException(nameof(result));
	}

	public PushDispatchResult Result { get; }

	public PushNotification Notification => Result.Notification;
}
