namespace PushRouter.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		Plugin.Maui.PushRouter.PushRouter.Current.MarkReady();
		return window;
	}
}
