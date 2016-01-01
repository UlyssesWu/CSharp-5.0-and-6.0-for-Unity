using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class UnitySynchronizationContext : SynchronizationContext
{
	private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

	public string Name { get; }

	public UnitySynchronizationContext(string name)
	{
		Name = name;
	}

	public override void Post(SendOrPostCallback d, object state)
	{
		queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
	}

	public void Run()
	{
		KeyValuePair<SendOrPostCallback, object> workItem;
		while (queue.TryTake(out workItem))
		{
			workItem.Key(workItem.Value);
		}
	}
}