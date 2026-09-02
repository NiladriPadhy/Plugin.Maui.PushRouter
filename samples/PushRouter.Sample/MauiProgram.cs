using Microsoft.Extensions.Logging;
using Plugin.Maui.PushRouter;

namespace PushRouter.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UsePushRouter(options =>
			{
				options.EnableLogging = true;
				options.NavigateOnTapOnly = true;
				options.AllowUnmappedPayloadRoutes = false;
				options.Map("order", "//order?id={orderId}");
				options.Map("chat", "//chat?thread={threadId}");
				options.Handle("silent", context =>
				{
					System.Diagnostics.Debug.WriteLine($"Silent push: {context.Notification.Body}");
				});
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
