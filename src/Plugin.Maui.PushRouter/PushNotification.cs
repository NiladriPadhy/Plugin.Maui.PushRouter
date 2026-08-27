namespace Plugin.Maui.PushRouter;

/// <summary>
/// A normalized FCM / APNs (or manually injected) notification.
/// </summary>
public sealed class PushNotification
{
	public PushNotification(
		IReadOnlyDictionary<string, string> data,
		PushOrigin origin = PushOrigin.Unknown,
		PushDelivery delivery = PushDelivery.Unknown,
		string? messageId = null,
		string? title = null,
		string? body = null,
		string? route = null,
		string? type = null,
		bool wasAppInForeground = false,
		DateTimeOffset? receivedAt = null,
		object? nativePayload = null)
	{
		Data = data ?? throw new ArgumentNullException(nameof(data));
		Origin = origin;
		Delivery = delivery;
		MessageId = messageId;
		Title = title;
		Body = body;
		Route = route;
		Type = type;
		WasAppInForeground = wasAppInForeground;
		ReceivedAt = receivedAt ?? DateTimeOffset.UtcNow;
		NativePayload = nativePayload;
	}

	/// <summary>
	/// Gets the flattened payload keys (FCM <c>data</c>, APNs custom keys, plus extracted alert fields).
	/// </summary>
	public IReadOnlyDictionary<string, string> Data { get; }

	public PushOrigin Origin { get; }

	public PushDelivery Delivery { get; }

	/// <summary>
	/// Gets the provider message id when present (<c>google.message_id</c>, APNs <c>gcm.message_id</c>, or a host-supplied id).
	/// </summary>
	public string? MessageId { get; }

	public string? Title { get; }

	public string? Body { get; }

	/// <summary>
	/// Gets the raw route value from the payload (handler key or Shell path).
	/// </summary>
	public string? Route { get; }

	/// <summary>
	/// Gets the payload type / action key when present.
	/// </summary>
	public string? Type { get; }

	public bool WasAppInForeground { get; }

	public DateTimeOffset ReceivedAt { get; }

	/// <summary>
	/// Gets the original platform object when the payload was parsed from Intent extras or APNs userInfo.
	/// </summary>
	public object? NativePayload { get; }

	/// <summary>
	/// Reads a data value using an ordinal-ignore-case key match.
	/// </summary>
	public string? this[string key] => PayloadLookup.Get(Data, key);

	/// <summary>
	/// Returns a copy with a different <see cref="Delivery"/> (used when a queued receive is later treated as a tap).
	/// </summary>
	public PushNotification WithDelivery(PushDelivery delivery) =>
		delivery == Delivery
			? this
			: new PushNotification(Data, Origin, delivery, MessageId, Title, Body, Route, Type, WasAppInForeground, ReceivedAt, NativePayload);

	public override string ToString()
	{
		var key = Route ?? Type ?? "(no route)";
		return $"{Origin}/{Delivery} {key} id={MessageId ?? "-"}";
	}
}
