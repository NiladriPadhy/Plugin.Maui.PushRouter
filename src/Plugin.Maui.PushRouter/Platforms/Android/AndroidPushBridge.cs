#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;

namespace Plugin.Maui.PushRouter;

static class AndroidPushBridge
{
	public static void HandleActivityIntent(Activity? activity)
	{
		if (activity?.Intent is null)
			return;

		HandleIntent(activity.Intent);
	}

	public static void HandleIntent(Intent? intent, PushDelivery delivery = PushDelivery.Tapped)
	{
		if (intent is null)
			return;

		var data = ToDictionary(intent);
		if (!PushNotificationParser.LooksLikePush(data, PushRouter.Current.Options))
			return;

		var notification = PushNotificationParser.Parse(
			data,
			PushRouter.Current.Options,
			delivery,
			PushOrigin.Fcm,
			wasAppInForeground: false,
			nativePayload: intent);

		PushRouter.Current.Handle(notification);
	}

	public static IReadOnlyDictionary<string, string> ToDictionary(Intent intent)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var extras = intent.Extras;
		if (extras is null)
			return result;

		var keys = extras.KeySet();
		if (keys is null)
			return result;

		foreach (var key in keys)
		{
			if (string.IsNullOrWhiteSpace(key))
				continue;

			var value = ReadExtra(extras, key);
			if (value is not null)
				result[key] = value;
		}

		return result;
	}

	static string? ReadExtra(Bundle extras, string key)
	{
		try
		{
			var text = extras.GetString(key);
			if (!string.IsNullOrEmpty(text))
				return text;
		}
		catch (Java.Lang.Exception)
		{
			// Non-string extra.
		}

		try
		{
			var value = extras.Get(key);
			return value?.ToString();
		}
		catch (Java.Lang.Exception)
		{
			return null;
		}
	}
}
#endif
