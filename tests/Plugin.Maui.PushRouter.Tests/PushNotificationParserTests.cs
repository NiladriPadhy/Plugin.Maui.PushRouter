namespace Plugin.Maui.PushRouter.Tests;

public sealed class PushNotificationParserTests
{
	[Fact]
	public void Parse_Fcm_Data_Extracts_Route_And_MessageId()
	{
		var notification = PushNotificationParser.Parse(RouterFactory.FcmOrder(), delivery: PushDelivery.Tapped);

		Assert.Equal(PushOrigin.Fcm, notification.Origin);
		Assert.Equal(PushDelivery.Tapped, notification.Delivery);
		Assert.Equal("fcm-1", notification.MessageId);
		Assert.Equal("order", notification.Route);
		Assert.Equal("Order shipped", notification.Title);
		Assert.Equal("Your order is on the way", notification.Body);
		Assert.Equal("42", notification["orderId"]);
	}

	[Fact]
	public void Parse_Apns_Flattens_Alert()
	{
		var notification = PushNotificationParser.Parse(RouterFactory.ApnsChat(), delivery: PushDelivery.Received);

		Assert.Equal(PushOrigin.Apns, notification.Origin);
		Assert.Equal("chat", notification.Route);
		Assert.Equal("New message", notification.Title);
		Assert.Equal("Hello from APNs", notification.Body);
	}

	[Fact]
	public void LooksLikePush_Requires_Route_Type_Or_Provider_Keys()
	{
		Assert.True(PushNotificationParser.LooksLikePush(RouterFactory.FcmOrder()));
		Assert.True(PushNotificationParser.LooksLikePush(new Dictionary<string, string> { ["type"] = "promo" }));
		Assert.False(PushNotificationParser.LooksLikePush(new Dictionary<string, string> { ["android.activity"] = "1" }));
		Assert.False(PushNotificationParser.LooksLikePush(new Dictionary<string, string>()));
	}

	[Fact]
	public void InferOrigin_Uses_Provider_Prefixes()
	{
		Assert.Equal(PushOrigin.Fcm, PushNotificationParser.InferOrigin(new Dictionary<string, string> { ["google.message_id"] = "1" }));
		Assert.Equal(PushOrigin.Apns, PushNotificationParser.InferOrigin(new Dictionary<string, string> { ["aps.alert"] = "hi" }));
		Assert.Equal(PushOrigin.Unknown, PushNotificationParser.InferOrigin(new Dictionary<string, string> { ["route"] = "home" }));
	}

	[Fact]
	public void Custom_Keys_Are_Honored()
	{
		var options = new PushRouterOptions
		{
			RouteKey = "screen",
			TypeKey = "action",
			TitleKey = "headline"
		};

		var notification = PushNotificationParser.Parse(
			new Dictionary<string, string>
			{
				["screen"] = "inbox",
				["action"] = "open",
				["headline"] = "Hello"
			},
			options);

		Assert.Equal("inbox", notification.Route);
		Assert.Equal("open", notification.Type);
		Assert.Equal("Hello", notification.Title);
	}
}
