namespace Plugin.Maui.PushRouter;

/// <summary>
/// Thrown when a notification cannot be routed or a screen cannot be opened.
/// </summary>
public sealed class PushRouterException : Exception
{
	public PushRouterException(PushRouterError error, string message, Exception? innerException = null)
		: base(message, innerException)
	{
		Error = error;
	}

	public PushRouterError Error { get; }
}
