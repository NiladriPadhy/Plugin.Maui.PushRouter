namespace Plugin.Maui.PushRouter;

/// <summary>
/// Identifies which push transport produced a notification.
/// </summary>
public enum PushOrigin
{
	/// <summary>
	/// Origin could not be inferred from the payload.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Firebase Cloud Messaging (Android, or FCM-delivered APNs).
	/// </summary>
	Fcm = 1,

	/// <summary>
	/// Apple Push Notification service.
	/// </summary>
	Apns = 2,

	/// <summary>
	/// Injected by the host (tests, in-app simulation, another SDK).
	/// </summary>
	Manual = 3
}
