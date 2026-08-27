namespace Plugin.Maui.PushRouter.Tests;

public sealed class RouteTemplateTests
{
	[Fact]
	public void Expand_Replaces_Placeholders()
	{
		var path = RouteTemplate.Expand(
			"//order?id={orderId}&ref={ref}",
			new Dictionary<string, string>
			{
				["orderId"] = "42",
				["ref"] = "email"
			});

		Assert.Equal("//order?id=42&ref=email", path);
	}

	[Fact]
	public void Expand_Escapes_Values()
	{
		var path = RouteTemplate.Expand(
			"//search?q={q}",
			new Dictionary<string, string> { ["q"] = "a b&c" });

		Assert.Equal("//search?q=a%20b%26c", path);
	}

	[Fact]
	public void Expand_Missing_Key_Is_Empty()
	{
		var path = RouteTemplate.Expand("//order?id={orderId}", new Dictionary<string, string>());
		Assert.Equal("//order?id=", path);
	}

	[Fact]
	public void Resolve_Uses_First_Segment_Of_Path()
	{
		Assert.Equal("order", RouteKeyResolver.FirstSegment("//order/details?id=1"));
		Assert.True(RouteKeyResolver.IsPath("//order"));
		Assert.False(RouteKeyResolver.IsPath("order"));
	}
}
