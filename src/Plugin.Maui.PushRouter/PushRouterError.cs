namespace Plugin.Maui.PushRouter;

/// <summary>
/// Classifies a <see cref="PushRouterException"/>.
/// </summary>
public enum PushRouterError
{
	/// <summary>
	/// The operation is not valid in the current state.
	/// </summary>
	InvalidOperation = 0,

	/// <summary>
	/// A route handler or navigator failed.
	/// </summary>
	DispatchFailure = 1,

	/// <summary>
	/// Shell (or the configured navigator) was not available.
	/// </summary>
	NavigationUnavailable = 2
}
