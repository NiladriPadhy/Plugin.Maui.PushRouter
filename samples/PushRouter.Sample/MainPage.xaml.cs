using Plugin.Maui.PushRouter;

namespace PushRouter.Sample;

public partial class MainPage : ContentPage
{
	readonly IPushRouter _router;
	readonly List<string> _logLines = [];
	int _messageSequence;

	public MainPage()
	{
		InitializeComponent();
		_router = Plugin.Maui.PushRouter.PushRouter.Current;
		_router.Received += OnReceived;
		_router.Tapped += OnTapped;
		_router.Routed += OnRouted;
		_router.Unhandled += OnUnhandled;
		_router.Failed += OnFailed;

		PlatformLabel.Text = _router.IsSupported
			? $"Platform: {_router.Platform.Transport} · taps captured: {_router.Platform.CapturesNotificationTaps}"
			: "Platform: not supported";
	}

	void OnFcmTapClicked(object? sender, EventArgs e)
	{
		_router.HandleTapped(new Dictionary<string, string>
		{
			["google.message_id"] = NextId("fcm"),
			["route"] = "order",
			["orderId"] = "1842",
			["title"] = "Order shipped",
			["body"] = "FCM data payload for order 1842"
		});
	}

	void OnApnsTapClicked(object? sender, EventArgs e)
	{
		_router.HandleTapped(new Dictionary<string, string>
		{
			["aps.alert.title"] = "New message",
			["aps.alert.body"] = "Alex: are you free?",
			["route"] = "chat",
			["threadId"] = "thread-22"
		});
	}

	void OnReceiveClicked(object? sender, EventArgs e)
	{
		_router.HandleReceived(new Dictionary<string, string>
		{
			["google.message_id"] = NextId("recv"),
			["route"] = "order",
			["orderId"] = "9001",
			["title"] = "Foreground FCM",
			["body"] = "Should log only — no navigation"
		});
	}

	void OnSilentClicked(object? sender, EventArgs e)
	{
		_router.HandleTapped(new Dictionary<string, string>
		{
			["route"] = "silent",
			["body"] = "Refresh badge / sync only"
		});
	}

	void OnUnknownClicked(object? sender, EventArgs e)
	{
		_router.HandleTapped(new Dictionary<string, string>
		{
			["route"] = "not-registered",
			["title"] = "Unknown"
		});
	}

	void OnPathClicked(object? sender, EventArgs e)
	{
		_router.HandleTapped(new Dictionary<string, string>
		{
			["route"] = "//order?id=direct",
			["title"] = "Direct path"
		});
	}

	void OnReceived(object? sender, PushNotificationEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			ShowLast(e.Notification);
			AppendLog($"RECEIVED {e.Notification}");
		});

	void OnTapped(object? sender, PushNotificationEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			ShowLast(e.Notification);
			AppendLog($"TAPPED {e.Notification}");
		});

	void OnRouted(object? sender, PushRoutedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() =>
		{
			AppendLog(e.Result.Navigated
				? $"ROUTED {e.Result.RouteKey} → {e.Result.NavigationRoute}"
				: $"HANDLED {e.Result.RouteKey} (no nav)");
		});

	void OnUnhandled(object? sender, PushUnhandledEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() => AppendLog($"UNHANDLED key={e.RouteKey}"));

	void OnFailed(object? sender, PushFailedEventArgs e) =>
		MainThread.BeginInvokeOnMainThread(() => AppendLog($"FAILED {e.Exception.Message}"));

	void ShowLast(PushNotification notification)
	{
		LastLabel.Text =
			$"{notification.Origin} / {notification.Delivery}{Environment.NewLine}" +
			$"Title: {notification.Title}{Environment.NewLine}" +
			$"Body: {notification.Body}{Environment.NewLine}" +
			$"Route: {notification.Route} · Type: {notification.Type}{Environment.NewLine}" +
			$"Id: {notification.MessageId ?? "-"}";
	}

	void AppendLog(string line)
	{
		_logLines.Insert(0, $"{DateTime.Now:HH:mm:ss} {line}");
		if (_logLines.Count > 40)
			_logLines.RemoveAt(_logLines.Count - 1);

		LogLabel.Text = string.Join(Environment.NewLine, _logLines);
	}

	string NextId(string prefix) => $"{prefix}-{Interlocked.Increment(ref _messageSequence)}";
}
