namespace Plugin.Maui.PushRouter;

/// <summary>
/// Handles a notification for a registered route / type key.
/// </summary>
public interface IPushRouteHandler
{
	Task<PushRouteResult> HandleAsync(PushRouteContext context, CancellationToken cancellationToken);
}
