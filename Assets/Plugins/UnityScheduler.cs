using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityScheduler : MonoBehaviour
{
	private UnitySynchronizationContext context;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);

		context = new UnitySynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(context);
	}

	private void LateUpdate()
	{
		context.Run();
	}
}

public class UnitySynchronizationContext : SynchronizationContext
{
	private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue =
		new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

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

internal class UnityTaskScheduler : TaskScheduler
{
	protected override IEnumerable<Task> GetScheduledTasks()
	{
		throw new NotImplementedException();
	}

	protected override void QueueTask(Task task)
	{
		throw new NotImplementedException();
	}

	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		throw new NotImplementedException();
	}
}