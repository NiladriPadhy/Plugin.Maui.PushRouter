namespace Plugin.Maui.PushRouter;

sealed class ProcessedIdCache(int capacity)
{
	readonly HashSet<string> _ids = new(StringComparer.Ordinal);
	readonly Queue<string> _order = new();

	public bool TryAdd(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
			return true;

		if (!_ids.Add(id))
			return false;

		_order.Enqueue(id);
		while (_order.Count > capacity)
		{
			var oldest = _order.Dequeue();
			_ids.Remove(oldest);
		}

		return true;
	}
}
