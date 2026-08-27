namespace PushRouter.Sample;

public partial class ChatPage : ContentPage, IQueryAttributable
{
	public ChatPage()
	{
		InitializeComponent();
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		var thread = query.TryGetValue("thread", out var value) ? value?.ToString() : "(missing)";
		DetailLabel.Text = $"Thread: {thread}";
	}
}
