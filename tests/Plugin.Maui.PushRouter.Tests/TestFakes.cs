namespace Plugin.Maui.PushRouter.Tests;

sealed class FakeNavigator : IPushNavigator
{
	public bool CanNavigate { get; set; } = true;

	public List<string> Routes { get; } = [];

	public Task NavigateAsync(string route, PushNotification notification, CancellationToken cancellationToken)
	{
		if (!CanNavigate)
		{
			throw new PushRouterException(
				PushRouterError.NavigationUnavailable,
				"Navigator is not ready.");
		}

		Routes.Add(route);
		return Task.CompletedTask;
	}
}

sealed class RecordingHandler : IPushRouteHandler
{
	public PushRouteResult Result { get; set; } = PushRouteResult.Handled;

	public List<PushRouteContext> Calls { get; } = [];

	public Task<PushRouteResult> HandleAsync(PushRouteContext context, CancellationToken cancellationToken)
	{
		Calls.Add(context);
		return Task.FromResult(Result);
	}
}

static class RouterFactory
{
	public static (IPushRouter Router, FakeNavigator Navigator) Create(
		Action<PushRouterOptions>? configure = null,
		bool ready = true)
	{
		var navigator = new FakeNavigator();
		var options = new PushRouterOptions
		{
			Navigator = navigator,
			QueueUntilReady = !ready,
			EnableLogging = false
		};
		configure?.Invoke(options);
		options.Navigator = navigator;

		var router = PushRouter.Create(options);
		if (ready)
			router.MarkReady();

		return (router, navigator);
	}

	public static Dictionary<string, string> FcmOrder(string orderId = "42", string? messageId = "fcm-1") => new()
	{
		["google.message_id"] = messageId ?? "",
		["route"] = "order",
		["orderId"] = orderId,
		["title"] = "Order shipped",
		["body"] = "Your order is on the way"
	};

	public static Dictionary<string, string> ApnsChat(string threadId = "t-9") => new()
	{
		["aps.alert.title"] = "New message",
		["aps.alert.body"] = "Hello from APNs",
		["route"] = "chat",
		["threadId"] = threadId
	};
}
