namespace Plugin.Maui.PushRouter.Tests;

public sealed class QueueAndDedupTests
{
	[Fact]
	public async Task Queues_Until_Ready()
	{
		var (router, navigator) = RouterFactory.Create(
			o =>
			{
				o.QueueUntilReady = true;
				o.Map("order", "//order?id={orderId}");
			},
			ready: false);

		var queued = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "m-queue"));

		Assert.True(queued.Queued);
		Assert.Empty(navigator.Routes);

		router.MarkReady();
		await Task.Delay(50);

		Assert.Equal(["//order?id=42"], navigator.Routes);
	}

	[Fact]
	public async Task Deduplicates_By_MessageId()
	{
		var (router, navigator) = RouterFactory.Create(o => o.Map("order", "//order?id={orderId}"));

		var first = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "same"));
		var second = await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "same"));

		Assert.True(first.Navigated);
		Assert.True(second.Duplicate);
		Assert.Single(navigator.Routes);
	}

	[Fact]
	public async Task Dedup_Can_Be_Disabled()
	{
		var (router, navigator) = RouterFactory.Create(o =>
		{
			o.Deduplicate = false;
			o.Map("order", "//order?id={orderId}");
		});

		await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "same"));
		await router.HandleTappedAsync(RouterFactory.FcmOrder(messageId: "same"));

		Assert.Equal(2, navigator.Routes.Count);
	}

	[Fact]
	public void Unregister_Removes_Handler_And_Map()
	{
		var (router, _) = RouterFactory.Create(o =>
		{
			o.Map("order", "//order");
			o.Handle("order", _ => { });
		});

		Assert.True(router.Unregister("order"));
		Assert.False(router.Unregister("order"));
	}
}
