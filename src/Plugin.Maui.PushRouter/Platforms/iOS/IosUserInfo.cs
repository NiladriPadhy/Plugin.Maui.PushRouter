#if IOS
using Foundation;

namespace Plugin.Maui.PushRouter;

static class IosUserInfo
{
	public static IReadOnlyDictionary<string, string> ToDictionary(NSDictionary? userInfo)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (userInfo is null)
			return result;

		Flatten(userInfo, result, prefix: null);
		return result;
	}

	static void Flatten(NSDictionary dictionary, Dictionary<string, string> output, string? prefix)
	{
		foreach (var key in dictionary.Keys)
		{
			var name = key.ToString();
			if (string.IsNullOrWhiteSpace(name))
				continue;

			var fullName = string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
			var value = dictionary.ObjectForKey(key);

			switch (value)
			{
				case NSDictionary nested:
					Flatten(nested, output, fullName);
					break;
				case NSArray array:
					output[fullName] = string.Join(",", array.ToArray().Select(item => item?.ToString() ?? string.Empty));
					break;
				default:
					if (value is not null)
						output[fullName] = value.ToString() ?? string.Empty;
					break;
			}
		}
	}
}
#endif
