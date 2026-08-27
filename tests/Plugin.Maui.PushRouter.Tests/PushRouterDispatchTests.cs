namespace Plugin.Maui.PushRouter.Tests;

public sealed class PushRouterDispatchTests
{
	[Fact]
	public async Task Tap_Maps_To_Shell_Route()
	{
		var (router, navigator) = RouterFactory.Create(o => o.Map("order", "//order?id={orderId}"));

		var result = await router.HandleTappedAsync(RouterFactory.FcmOrder("99", messageId: "m-map"));

		Assert.True(result.Navigated);
		Assert.Equal("order", result.RouteKey);
		Assert.Equal("//order?id=99", result.NavigationRoute);
		Assert.Equal(["//order?id=99"], navigator.Routes);
	}

	[Fact]
	public async Task Handler_Can_Stop_Navigation()
	{
		var handler = new RecordingHandler { Result = PushRouteResult.Handled };
		var (router, navigator) = RouterFactory.Create(o =>
		{
			o.Map("order", "//order?id={orderId}");
			o.Handle("order", handler);
		});

		var result = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "m-handled"));

		Assert.True(result.Handled);
		Assert.False(result.Navigated);
		Assert.Empty(navigator.Routes);
		Assert.Single(handler.Calls);
	}

	[Fact]
	public async Task Handler_Can_Request_Navigation()
	{
		var (router, navigator) = RouterFactory.Create(o =>
		{
			o.Map("order", "//order?id={orderId}");
			o.Handle("order", (_, _) => Task.FromResult(PushRouteResult.Navigate));
		});

		var result = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "m-nav"));

		Assert.True(result.Navigated);
		Assert.Equal(["//order?id=42"], navigator.Routes);
	}

	[Fact]
	public async Task Receive_Does_Not_Navigate_When_Tap_Only()
	{
		var (router, navigator) = RouterFactory.Create(o => o.Map("chat", "//chat?thread={threadId}"));

		var result = await router.HandleReceivedAsync(RouterFactory.ApnsChat());

		Assert.True(result.Handled);
		Assert.False(result.Navigated);
		Assert.Empty(navigator.Routes);
	}

	[Fact]
	public async Task Receive_Navigates_When_Tap_Only_Disabled()
	{
		var (router, navigator) = RouterFactory.Create(o =>
		{
			o.NavigateOnTapOnly = false;
			o.Map("chat", "//chat?thread={threadId}");
		});

		var result = await router.HandleReceivedAsync(RouterFactory.ApnsChat());

		Assert.True(result.Navigated);
		Assert.Equal(["//chat?thread=t-9"], navigator.Routes);
	}

	[Fact]
	public async Task Path_In_Route_Is_Used_Directly()
	{
		var (router, navigator) = RouterFactory.Create();

		var result = await router.HandleTappedAsync(new Dictionary<string, string>
		{
			["route"] = "//inbox/detail?id=7",
			["google.message_id"] = "m-path"
		});

		Assert.True(result.Navigated);
		Assert.Equal("inbox", result.RouteKey);
		Assert.Equal("//inbox/detail?id=7", result.NavigationRoute);
		Assert.Equal(["//inbox/detail?id=7"], navigator.Routes);
	}

	[Fact]
	public async Task Unknown_Tap_Raises_Unhandled()
	{
		var (router, _) = RouterFactory.Create();
		PushUnhandledEventArgs? unhandled = null;
		router.Unhandled += (_, e) => unhandled = e;

		var result = await router.HandleTappedAsync(new Dictionary<string, string>
		{
			["route"] = "unknown",
			["google.message_id"] = "m-unknown"
		});

		Assert.False(result.Handled);
		Assert.False(result.Navigated);
		Assert.NotNull(unhandled);
		Assert.Equal("unknown", unhandled!.RouteKey);
	}

	[Fact]
	public async Task Default_Route_Is_Used_When_Unmapped()
	{
		var (router, navigator) = RouterFactory.Create(o => o.DefaultRoute = "//inbox");

		var result = await router.HandleTappedAsync(new Dictionary<string, string>
		{
			["type"] = "promo",
			["google.message_id"] = "m-default"
		});

		Assert.True(result.Navigated);
		Assert.Equal(["//inbox"], navigator.Routes);
	}

	[Fact]
	public async Task Events_Fire_For_Tap_And_Route()
	{
		var (router, _) = RouterFactory.Create(o => o.Map("order", "//order?id={orderId}"));
		var tapped = 0;
		var routed = 0;
		router.Tapped += (_, _) => tapped++;
		router.Routed += (_, _) => routed++;

		await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "m-events"));

		Assert.Equal(1, tapped);
		Assert.Equal(1, routed);
	}

	[Fact]
	public async Task Failed_Navigation_Raises_Failed()
	{
		var (router, navigator) = RouterFactory.Create(o => o.Map("order", "//order?id={orderId}"));
		navigator.CanNavigate = false;
		PushFailedEventArgs? failed = null;
		router.Failed += (_, e) => failed = e;

		var result = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "m-fail"));

		Assert.False(result.Navigated);
		Assert.NotNull(failed);
		Assert.IsType<PushRouterException>(failed!.Exception);
	}
}
