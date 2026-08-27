using Microsoft.Extensions.Logging;

namespace Plugin.Maui.PushRouter;

sealed class MicrosoftLoggerAdapter(ILogger logger) : IPushRouterLogger
{
	public void Log(PushRouterLogLevel level, string message, Exception? exception = null)
	{
		logger.Log(ToLogLevel(level), exception, "{Message}", message);
	}

	static LogLevel ToLogLevel(PushRouterLogLevel level) => level switch
	{
		PushRouterLogLevel.Trace => LogLevel.Trace,
		PushRouterLogLevel.Debug => LogLevel.Debug,
		PushRouterLogLevel.Information => LogLevel.Information,
		PushRouterLogLevel.Warning => LogLevel.Warning,
		PushRouterLogLevel.Error => LogLevel.Error,
		_ => LogLevel.Information
	};
}
