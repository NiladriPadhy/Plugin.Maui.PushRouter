namespace Plugin.Maui.PushRouter;

/// <summary>
/// Normalizes FCM data maps and APNs userInfo dictionaries into <see cref="PushNotification"/>.
/// </summary>
public static class PushNotificationParser
{
	static readonly string[] TitleKeys =
	[
		"title",
		"aps.alert.title",
		"alert.title",
		"gcm.notification.title",
		"gcm.n.title",
		"notification.title",
		"notification_title"
	];

	static readonly string[] BodyKeys =
	[
		"body",
		"message",
		"aps.alert.body",
		"aps.alert",
		"alert.body",
		"alert",
		"gcm.notification.body",
		"gcm.n.body",
		"notification.body",
		"notification_body"
	];

	static readonly string[] MessageIdKeys =
	[
		"google.message_id",
		"gcm.message_id",
		"message_id",
		"messageId",
		"gcm.messageId"
	];

	/// <summary>
	/// Parses a flattened string dictionary. Works for FCM <c>RemoteMessage.Data</c>,
	/// Android Intent extras, APNs userInfo, or a host-built map.
	/// </summary>
	public static PushNotification Parse(
		IReadOnlyDictionary<string, string> data,
		PushRouterOptions? options = null,
		PushDelivery delivery = PushDelivery.Unknown,
		PushOrigin? origin = null,
		bool wasAppInForeground = false,
		object? nativePayload = null)
	{
		ArgumentNullException.ThrowIfNull(data);
		options ??= new PushRouterOptions();

		var resolvedOrigin = origin ?? InferOrigin(data);
		var title = PayloadLookup.Get(data, options.TitleKey) ?? PayloadLookup.GetAny(data, TitleKeys);
		var body = PayloadLookup.Get(data, options.BodyKey) ?? PayloadLookup.GetAny(data, BodyKeys);
		var route = PayloadLookup.Get(data, options.RouteKey);
		var type = PayloadLookup.Get(data, options.TypeKey);
		var messageId = PayloadLookup.Get(data, options.MessageIdKey) ?? PayloadLookup.GetAny(data, MessageIdKeys);

		if (string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(body))
			title = type;

		return new PushNotification(
			data,
			resolvedOrigin,
			delivery,
			messageId,
			title,
			body,
			route,
			type,
			wasAppInForeground,
			DateTimeOffset.UtcNow,
			nativePayload);
	}

	/// <summary>
	/// Returns <c>true</c> when the dictionary looks like an FCM or APNs payload rather than a generic activity launch.
	/// </summary>
	public static bool LooksLikePush(IReadOnlyDictionary<string, string> data, PushRouterOptions? options = null)
	{
		if (data is null || data.Count == 0)
			return false;

		options ??= new PushRouterOptions();

		if (PayloadLookup.Has(data, options.RouteKey) || PayloadLookup.Has(data, options.TypeKey))
			return true;

		if (PayloadLookup.GetAny(data, MessageIdKeys) is not null)
			return true;

		foreach (var key in data.Keys)
		{
			if (key.StartsWith("aps", StringComparison.OrdinalIgnoreCase)
				|| key.StartsWith("gcm.notification", StringComparison.OrdinalIgnoreCase)
				|| key.StartsWith("google.message", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	public static PushOrigin InferOrigin(IReadOnlyDictionary<string, string> data)
	{
		foreach (var key in data.Keys)
		{
			if (key.StartsWith("aps", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(key, "alert", StringComparison.OrdinalIgnoreCase))
			{
				return PushOrigin.Apns;
			}

			if (key.StartsWith("google.", StringComparison.OrdinalIgnoreCase) ||
				key.StartsWith("gcm.", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(key, "from", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(key, "collapse_key", StringComparison.OrdinalIgnoreCase))
			{
				return PushOrigin.Fcm;
			}
		}

		return PushOrigin.Unknown;
	}
}
