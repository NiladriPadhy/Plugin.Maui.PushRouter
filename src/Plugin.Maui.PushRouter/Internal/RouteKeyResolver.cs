namespace Plugin.Maui.PushRouter;

static class RouteKeyResolver
{
	public static string? Resolve(PushNotification notification, PushRouterOptions options)
	{
		var raw = notification.Route
			?? PayloadLookup.Get(notification.Data, options.RouteKey)
			?? notification.Type
			?? PayloadLookup.Get(notification.Data, options.TypeKey);

		if (string.IsNullOrWhiteSpace(raw))
			return options.DefaultRouteKey;

		return IsPath(raw) ? FirstSegment(raw) : raw.Trim();
	}

	public static bool IsPath(string value)
	{
		var trimmed = value.Trim();
		return trimmed.StartsWith('/') || trimmed.Contains('/', StringComparison.Ordinal);
	}

	public static string FirstSegment(string path)
	{
		var trimmed = path.Trim().TrimStart('/');
		var query = trimmed.IndexOf('?', StringComparison.Ordinal);
		if (query >= 0)
			trimmed = trimmed[..query];

		var slash = trimmed.IndexOf('/', StringComparison.Ordinal);
		return slash >= 0 ? trimmed[..slash] : trimmed;
	}
}
