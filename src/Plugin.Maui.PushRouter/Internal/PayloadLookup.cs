namespace Plugin.Maui.PushRouter;

static class PayloadLookup
{
	public static string? Get(IReadOnlyDictionary<string, string> data, string? key)
	{
		if (data is null || string.IsNullOrWhiteSpace(key))
			return null;

		if (data.TryGetValue(key, out var exact) && !string.IsNullOrWhiteSpace(exact))
			return exact;

		foreach (var pair in data)
		{
			if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(pair.Value))
				return pair.Value;
		}

		return null;
	}

	public static string? GetAny(IReadOnlyDictionary<string, string> data, params string[] keys)
	{
		foreach (var key in keys)
		{
			var value = Get(data, key);
			if (!string.IsNullOrWhiteSpace(value))
				return value;
		}

		return null;
	}

	public static bool Has(IReadOnlyDictionary<string, string> data, string? key) =>
		!string.IsNullOrWhiteSpace(Get(data, key));
}
