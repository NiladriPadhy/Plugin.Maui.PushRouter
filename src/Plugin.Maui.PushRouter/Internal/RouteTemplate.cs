using System.Text.RegularExpressions;

namespace Plugin.Maui.PushRouter;

static partial class RouteTemplate
{
	public static string Expand(string template, IReadOnlyDictionary<string, string> data)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(template);
		ArgumentNullException.ThrowIfNull(data);

		return PlaceholderRegex().Replace(template, match =>
		{
			var key = match.Groups[1].Value;
			var value = PayloadLookup.Get(data, key);
			return value is null ? string.Empty : Uri.EscapeDataString(value);
		});
	}

	[GeneratedRegex(@"\{([^{}]+)\}", RegexOptions.CultureInvariant)]
	private static partial Regex PlaceholderRegex();
}
