using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UnityTaskScheduler : TaskScheduler
{
	public readonly BlockingCollection<Task> mainThreadQueue = new BlockingCollection<Task>();

	protected override IEnumerable<Task> GetScheduledTasks()
	{
		return mainThreadQueue;
	}

	protected override void QueueTask(Task task)
	{
		mainThreadQueue.Add(task);
	}

	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		if (Thread.CurrentThread.ManagedThreadId != UnityScheduler.MainThreadId)
		{
			return false;
		}

		return TryExecuteTask(task);
	}

	public void ExecuteTask(Task task)
	{
		var result = TryExecuteTask(task);
		if (result == false)
		{
			throw new InvalidOperationException();
		}
	}
}