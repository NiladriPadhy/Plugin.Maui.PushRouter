using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Plugin.Maui.PushRouter;

sealed class PushRouterInitializer : IMauiInitializeService
{
	public void Initialize(IServiceProvider services)
	{
		var options = services.GetService<PushRouterOptions>() ?? new PushRouterOptions();
		var router = services.GetService<IPushRouter>() ?? PushRouter.Current;

		if (router is PushRouterImplementation implementation)
			implementation.Services = services;

		if (options.EnableLogging)
		{
			var logger = options.Logger
				?? MauiAppBuilderExtensions.CreateLoggerAdapter(services)
				?? new DebugPushRouterLogger();
			router.EnableLogging(true, logger);
		}

		MainThread.BeginInvokeOnMainThread(() =>
		{
			if (Shell.Current is not null)
				router.MarkReady();
		});
	}
}
