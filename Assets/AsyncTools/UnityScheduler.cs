using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SyncronizationContextQueue = System.Collections.Concurrent.BlockingCollection<System.Collections.Generic.KeyValuePair<System.Threading.SendOrPostCallback, object>>;

public class UnityScheduler : MonoBehaviour
{
	public static UnityScheduler Instance { get; private set; }
	public static readonly UnityTaskScheduler MainThread = new UnityTaskScheduler();
	public static int MainThreadId { get; private set; }

	private UnitySynchronizationContext synchronizationContext;

	private void Awake()
	{
		if (Instance != null)
		{
			throw new NotSupportedException("UnityScheduler already exists.");
		}
		Instance = this;
		MainThreadId = Thread.CurrentThread.ManagedThreadId;

		DontDestroyOnLoad(gameObject);
		synchronizationContext = new UnitySynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(synchronizationContext);
	}

	private void Update()
	{
		Task task;
		while (MainThread.mainThreadQueue.TryTake(out task))
		{
			MainThread.ExecuteTask(task);
		}

		synchronizationContext?.Run(); // execute continuations
	}

	#region Nested classes

	private class UnitySynchronizationContext : SynchronizationContext
	{
		private readonly SyncronizationContextQueue queue = new SyncronizationContextQueue();

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
			if (Thread.CurrentThread.ManagedThreadId != MainThreadId)
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

	#endregion
}