namespace Plugin.Maui.PushRouter;

/// <summary>
/// Describes how notifications are captured on the current target.
/// </summary>
public sealed class PushRouterPlatformInfo
{
	public PushRouterPlatformInfo(bool isSupported, string transport, bool capturesNotificationTaps)
	{
		IsSupported = isSupported;
		Transport = transport;
		CapturesNotificationTaps = capturesNotificationTaps;
	}

	/// <summary>
	/// Gets a value indicating whether routing is available on this target.
	/// Always <c>true</c> for Android, iOS, and the shared <c>net10.0</c> surface.
	/// </summary>
	public bool IsSupported { get; }

	/// <summary>
	/// Gets the native transport name, such as <c>FCM</c> or <c>APNs</c>.
	/// </summary>
	public string Transport { get; }

	/// <summary>
	/// Gets a value indicating whether <c>UsePushRouter</c> hooks OS tap / cold-start payloads.
	/// </summary>
	public bool CapturesNotificationTaps { get; }

	internal static PushRouterPlatformInfo Current =>
#if ANDROID
		new(true, "FCM", true);
#elif IOS
		new(true, "APNs", true);
#else
		new(true, "Manual", false);
#endif
}
