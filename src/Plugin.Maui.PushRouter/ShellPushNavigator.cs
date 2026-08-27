namespace Plugin.Maui.PushRouter;

/// <summary>
/// Navigates using <see cref="Shell.Current"/>.
/// </summary>
public sealed class ShellPushNavigator : IPushNavigator
{
	public bool CanNavigate => Shell.Current is not null;

	public Task NavigateAsync(string route, PushNotification notification, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(route);
		ArgumentNullException.ThrowIfNull(notification);

		if (Shell.Current is null)
		{
			throw new PushRouterException(
				PushRouterError.NavigationUnavailable,
				"Shell.Current is null. Register routes on AppShell and call PushRouter.Current.MarkReady() after the window is created.");
		}

		cancellationToken.ThrowIfCancellationRequested();
		return Shell.Current.GoToAsync(route);
	}
}
