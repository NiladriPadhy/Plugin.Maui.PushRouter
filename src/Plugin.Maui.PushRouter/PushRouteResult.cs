namespace Plugin.Maui.PushRouter;

/// <summary>
/// Result returned by an <see cref="IPushRouteHandler"/>.
/// </summary>
public enum PushRouteResult
{
	/// <summary>
	/// The handler finished the notification. No Shell navigation is performed.
	/// </summary>
	Handled = 0,

	/// <summary>
	/// The handler wants the router to continue with mapped Shell navigation.
	/// </summary>
	Navigate = 1,

	/// <summary>
	/// The handler declined. The router tries maps, then the default route.
	/// </summary>
	NotHandled = 2,

	/// <summary>
	/// Drop the notification. No navigation and no <c>Unhandled</c> event.
	/// </summary>
	Ignore = 3
}
