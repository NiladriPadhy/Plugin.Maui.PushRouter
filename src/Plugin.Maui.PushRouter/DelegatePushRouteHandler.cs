namespace Plugin.Maui.PushRouter;

/// <summary>
/// Adapts a delegate to <see cref="IPushRouteHandler"/>.
/// </summary>
public sealed class DelegatePushRouteHandler : IPushRouteHandler
{
	readonly Func<PushRouteContext, CancellationToken, Task<PushRouteResult>> _handler;

	public DelegatePushRouteHandler(Func<PushRouteContext, CancellationToken, Task<PushRouteResult>> handler)
	{
		_handler = handler ?? throw new ArgumentNullException(nameof(handler));
	}

	public DelegatePushRouteHandler(Func<PushRouteContext, Task<PushRouteResult>> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = (context, _) => handler(context);
	}

	public DelegatePushRouteHandler(Action<PushRouteContext> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_handler = (context, _) =>
		{
			handler(context);
			return Task.FromResult(PushRouteResult.Handled);
		};
	}

	public Task<PushRouteResult> HandleAsync(PushRouteContext context, CancellationToken cancellationToken) =>
		_handler(context, cancellationToken);
}
