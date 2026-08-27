#if IOS
using UserNotifications;

namespace Plugin.Maui.PushRouter;

/// <summary>
/// Forwards APNs presentation and tap callbacks to <see cref="PushRouter"/>.
/// When an existing <see cref="IUNUserNotificationCenterDelegate"/> is present, it is wrapped and still invoked.
/// </summary>
public sealed class PushRouterNotificationDelegate : UNUserNotificationCenterDelegate
{
	readonly IUNUserNotificationCenterDelegate? _inner;

	public PushRouterNotificationDelegate(IUNUserNotificationCenterDelegate? inner = null)
	{
		_inner = inner;
	}

	public override void WillPresentNotification(
		UNUserNotificationCenter center,
		UNNotification notification,
		Action<UNNotificationPresentationOptions> completionHandler)
	{
		IosPushBridge.HandleNotification(notification, PushDelivery.Received, wasAppInForeground: true);

		if (_inner is UNUserNotificationCenterDelegate innerDelegate)
		{
			innerDelegate.WillPresentNotification(center, notification, completionHandler);
			return;
		}

		completionHandler(
			UNNotificationPresentationOptions.Banner
			| UNNotificationPresentationOptions.List
			| UNNotificationPresentationOptions.Sound
			| UNNotificationPresentationOptions.Badge);
	}

	public override void DidReceiveNotificationResponse(
		UNUserNotificationCenter center,
		UNNotificationResponse response,
		Action completionHandler)
	{
		IosPushBridge.HandleNotification(response.Notification, PushDelivery.Tapped, wasAppInForeground: false);

		if (_inner is UNUserNotificationCenterDelegate innerDelegate)
		{
			innerDelegate.DidReceiveNotificationResponse(center, response, completionHandler);
			return;
		}

		completionHandler();
	}
}
#endif
