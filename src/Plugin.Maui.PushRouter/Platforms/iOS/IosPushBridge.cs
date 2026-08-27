#if IOS
using Foundation;
using UIKit;
using UserNotifications;

namespace Plugin.Maui.PushRouter;

static class IosPushBridge
{
	static bool _attached;

	public static void AttachNotificationCenter()
	{
		if (_attached)
			return;

		var center = UNUserNotificationCenter.Current;
		if (center.Delegate is PushRouterNotificationDelegate)
		{
			_attached = true;
			return;
		}

		center.Delegate = new PushRouterNotificationDelegate(center.Delegate);
		_attached = true;
	}

	public static void HandleLaunchOptions(NSDictionary? launchOptions)
	{
		if (launchOptions is null)
			return;

		NSObject? payload = null;
		if (launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
			payload = launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey);

		if (payload is not NSDictionary userInfo)
			return;

		HandleUserInfo(userInfo, PushDelivery.Tapped, wasAppInForeground: false);
	}

	public static void HandleNotification(UNNotification? notification, PushDelivery delivery, bool wasAppInForeground)
	{
		if (notification?.Request?.Content?.UserInfo is null)
			return;

		HandleUserInfo(notification.Request.Content.UserInfo, delivery, wasAppInForeground, notification);
	}

	public static void HandleUserInfo(
		NSDictionary? userInfo,
		PushDelivery delivery,
		bool wasAppInForeground,
		object? nativePayload = null)
	{
		if (userInfo is null)
			return;

		var data = IosUserInfo.ToDictionary(userInfo);
		if (!PushNotificationParser.LooksLikePush(data, PushRouter.Current.Options))
			return;

		var notification = PushNotificationParser.Parse(
			data,
			PushRouter.Current.Options,
			delivery,
			PushOrigin.Apns,
			wasAppInForeground,
			nativePayload ?? userInfo);

		PushRouter.Current.Handle(notification);
	}
}
#endif
