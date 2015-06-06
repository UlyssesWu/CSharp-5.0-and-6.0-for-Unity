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

	private readonly BlockingCollection<Task> mainThreadQueue = new BlockingCollection<Task>();
	private UnitySynchronizationContext synchronizationContext;

	public void EnqueueTask(Task task)
	{
		mainThreadQueue.Add(task);
	}

	private void Awake()
	{
		if (Instance != null)
		{
			throw new NotSupportedException("UnityScheduler already exists.");
		}
		Instance = this;

		DontDestroyOnLoad(gameObject);
		synchronizationContext = new UnitySynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(synchronizationContext);
	}

	private void Update()
	{
		Task task;
		while (mainThreadQueue.TryTake(out task))
		{
			MainThread.TryExecuteTask(task);
		}

		synchronizationContext.Run(); // execute continuations
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
		protected override IEnumerable<Task> GetScheduledTasks()
		{
			throw new NotImplementedException();
		}

		protected override void QueueTask(Task task)
		{
			Instance.EnqueueTask(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return false;
		}

		public new bool TryExecuteTask(Task task)
		{
			return base.TryExecuteTask(task);
		}
	}
	#endregion
}