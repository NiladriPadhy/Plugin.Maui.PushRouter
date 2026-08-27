namespace Plugin.Maui.PushRouter;

/// <summary>
/// Receives diagnostic messages from the PushRouter plugin.
/// </summary>
public interface IPushRouterLogger
{
	void Log(PushRouterLogLevel level, string message, Exception? exception = null);
}
