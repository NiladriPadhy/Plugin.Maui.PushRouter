namespace Plugin.Maui.PushRouter;

/// <summary>
/// How the notification reached the app.
/// </summary>
public enum PushDelivery
{
	/// <summary>
	/// Delivery mode was not specified.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The payload arrived while the app could observe it (typically foreground).
	/// </summary>
	Received = 1,

	/// <summary>
	/// The user opened the app from the system notification (tap or cold start).
	/// </summary>
	Tapped = 2
}
