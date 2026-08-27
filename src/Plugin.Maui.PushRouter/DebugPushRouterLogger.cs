using System.Diagnostics;

namespace Plugin.Maui.PushRouter;

/// <summary>
/// Writes plugin diagnostics to <see cref="Debug.WriteLine(string?)"/>.
/// </summary>
public sealed class DebugPushRouterLogger : IPushRouterLogger
{
	public void Log(PushRouterLogLevel level, string message, Exception? exception = null)
	{
		var line = exception is null
			? $"[PushRouter] {level}: {message}"
			: $"[PushRouter] {level}: {message}{Environment.NewLine}{exception}";

		Debug.WriteLine(line);
	}
}
