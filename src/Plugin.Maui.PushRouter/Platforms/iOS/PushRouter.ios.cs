#if IOS
using Foundation;
using UserNotifications;

namespace Plugin.Maui.PushRouter;

public static partial class PushRouter
{
	/// <summary>
	/// Attaches a <see cref="UNUserNotificationCenter"/> delegate that forwards APNs events to the router.
	/// Existing delegates are wrapped. <c>UsePushRouter</c> does this automatically.
	/// </summary>
	public static void AttachNotificationCenter() =>
		IosPushBridge.AttachNotificationCenter();

	/// <summary>
	/// Routes a flattened APNs userInfo dictionary (tap, receive, or cold start).
	/// </summary>
	public static void HandleUserInfo(NSDictionary? userInfo, PushDelivery delivery = PushDelivery.Tapped) =>
		IosPushBridge.HandleUserInfo(userInfo, delivery, wasAppInForeground: delivery == PushDelivery.Received);

	/// <summary>
	/// Routes <see cref="UIKit.UIApplication.LaunchOptionsRemoteNotificationKey"/> from FinishedLaunching.
	/// </summary>
	public static void HandleLaunchOptions(NSDictionary? launchOptions) =>
		IosPushBridge.HandleLaunchOptions(launchOptions);

	/// <summary>
	/// Flattens an APNs userInfo dictionary, including nested <c>aps.alert</c> keys.
	/// </summary>
	public static IReadOnlyDictionary<string, string> ReadUserInfo(NSDictionary userInfo) =>
		IosUserInfo.ToDictionary(userInfo);
}
#endif
