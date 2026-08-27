using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.PushRouter;

/// <summary>
/// Registers the PushRouter plugin with the MAUI dependency injection container.
/// </summary>
public static class MauiAppBuilderExtensions
{
	/// <summary>
	/// Adds <see cref="IPushRouter"/> as a singleton and wires Android/iOS notification tap hooks.
	/// </summary>
	/// <example>
	/// <code>
	/// builder.UsePushRouter(options =>
	/// {
	///     options.EnableLogging = true;
	///     options.Map("order", "//order?id={orderId}");
	/// });
	/// </code>
	/// </example>
	public static MauiAppBuilder UsePushRouter(this MauiAppBuilder builder, Action<PushRouterOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		var options = new PushRouterOptions();
		configure?.Invoke(options);

		builder.Services.AddSingleton(options);
		builder.Services.AddSingleton<IPushRouter>(services =>
		{
			options.Logger ??= CreateLoggerAdapter(services);
			var router = PushRouter.Create(options, services);
			PushRouter.SetDefault(router);
			return router;
		});
		builder.Services.AddTransient<IMauiInitializeService, PushRouterInitializer>();

		builder.ConfigureLifecycleEvents(events =>
		{
#if ANDROID
			events.AddAndroid(android =>
			{
				android.OnCreate((activity, _) => AndroidPushBridge.HandleActivityIntent(activity));
				android.OnNewIntent((activity, intent) =>
				{
					if (intent is not null)
						activity.Intent = intent;
					AndroidPushBridge.HandleIntent(intent);
				});
				android.OnResume(_ => TryMarkReady());
			});
#elif IOS
			events.AddiOS(ios =>
			{
				ios.FinishedLaunching((app, launchOptions) =>
				{
					IosPushBridge.AttachNotificationCenter();
					IosPushBridge.HandleLaunchOptions(launchOptions);
					return false;
				});
				ios.OnActivated(_ => TryMarkReady());
			});
#endif
		});

		return builder;
	}

	static void TryMarkReady()
	{
		if (Shell.Current is not null)
			PushRouter.Current.MarkReady();
	}

	internal static IPushRouterLogger? CreateLoggerAdapter(IServiceProvider serviceProvider)
	{
		var factory = serviceProvider.GetService<ILoggerFactory>();
		return factory is null ? null : new MicrosoftLoggerAdapter(factory.CreateLogger("Plugin.Maui.PushRouter"));
	}
}
