namespace Plugin.Maui.PushRouter;

/// <summary>
/// Entry point for the PushRouter plugin when dependency injection is not used.
/// </summary>
public static partial class PushRouter
{
	static IPushRouter? _current;

	/// <summary>
	/// Gets the shared <see cref="IPushRouter"/> instance.
	/// </summary>
	public static IPushRouter Current => _current ??= Create(new PushRouterOptions());

	/// <summary>
	/// Creates a new router. Used by <c>UsePushRouter</c> and tests.
	/// </summary>
	public static IPushRouter Create(PushRouterOptions? options = null, IServiceProvider? services = null) =>
		new PushRouterImplementation(options ?? new PushRouterOptions(), services);

	/// <summary>
	/// Replaces the shared instance. Intended for tests and custom implementations.
	/// </summary>
	public static void SetDefault(IPushRouter implementation) =>
		_current = implementation ?? throw new ArgumentNullException(nameof(implementation));
}
