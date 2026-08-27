namespace Plugin.Maui.PushRouter;

/// <summary>
/// Opens a screen for a routed notification. The default implementation uses MAUI Shell.
/// </summary>
public interface IPushNavigator
{
	/// <summary>
	/// Gets a value indicating whether a navigation host is currently available.
	/// </summary>
	bool CanNavigate { get; }

	/// <summary>
	/// Navigates to <paramref name="route"/> (a Shell path such as <c>//order?id=123</c>).
	/// </summary>
	Task NavigateAsync(string route, PushNotification notification, CancellationToken cancellationToken);
}
