using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityScheduler : MonoBehaviour
{
	public static UnityScheduler Instance { get; private set; }
	public static int MainThreadId { get; private set; }
	public static UnityTaskScheduler MainThreadScheduler { get; private set; }
	public static TaskScheduler ThreadPoolScheduler => TaskScheduler.Default;
	public static UnitySynchronizationContext UpdateContext { get; private set; }
	public static UnitySynchronizationContext LateUpdateContext { get; private set; }
	public static UnitySynchronizationContext FixedUpdateContext { get; private set; }

	private void Awake()
	{
		if (Instance != null)
		{
			throw new NotSupportedException("UnityScheduler already exists.");
		}
		Instance = this;
		MainThreadId = Thread.CurrentThread.ManagedThreadId;
		MainThreadScheduler = new UnityTaskScheduler();

		DontDestroyOnLoad(gameObject);

		UpdateContext = new UnitySynchronizationContext("Update");
		LateUpdateContext = new UnitySynchronizationContext("LateUpdate");
		FixedUpdateContext = new UnitySynchronizationContext("FixedUpdate");
		SynchronizationContext.SetSynchronizationContext(UpdateContext);
	}

	private void Update()
	{
		SynchronizationContext.SetSynchronizationContext(UpdateContext);

		Task task;
		while (MainThreadScheduler.mainThreadQueue.TryTake(out task))
		{
			MainThreadScheduler.ExecuteTask(task); // run scheduled tasks
		}

		UpdateContext?.Run(); // run pending continuations
	}

	private void LateUpdate()
	{
		SynchronizationContext.SetSynchronizationContext(LateUpdateContext);
		LateUpdateContext?.Run(); // run pending continuations
	}

	private void FixedUpdate()
	{
		SynchronizationContext.SetSynchronizationContext(FixedUpdateContext);
		FixedUpdateContext?.Run(); // run pending continuations
	}
}