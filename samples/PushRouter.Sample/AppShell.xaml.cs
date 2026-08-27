namespace PushRouter.Sample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("order", typeof(OrderPage));
		Routing.RegisterRoute("chat", typeof(ChatPage));
	}
}
