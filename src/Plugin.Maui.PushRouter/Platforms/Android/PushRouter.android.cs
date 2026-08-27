#if ANDROID
using Android.Content;

namespace Plugin.Maui.PushRouter;

public static partial class PushRouter
{
	/// <summary>
	/// Routes FCM extras from an Android activity <see cref="Intent"/> (notification tap or cold start).
	/// </summary>
	public static void HandleIntent(Intent? intent, PushDelivery delivery = PushDelivery.Tapped) =>
		AndroidPushBridge.HandleIntent(intent, delivery);

	/// <summary>
	/// Flattens Intent extras into a string dictionary (FCM <c>data</c> plus Google metadata).
	/// </summary>
	public static IReadOnlyDictionary<string, string> ReadIntent(Intent intent) =>
		AndroidPushBridge.ToDictionary(intent);
}
#endif
