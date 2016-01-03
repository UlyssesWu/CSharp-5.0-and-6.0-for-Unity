using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityScheduler : MonoBehaviour
{
	public static UnityScheduler Instance { get; private set; }
	public static int MainThreadId { get; private set; }

	/// <summary>
	/// Use UpdateScheduler, LateUpdateScheduler or FixedUpdateScheduler instead.
	/// </summary>
	[Obsolete]
	public static UnityTaskScheduler MainThreadScheduler => UpdateScheduler;

	/// <summary>
	/// Executes tasks in the main thread, Update context.
	/// </summary>
	public static UnityTaskScheduler UpdateScheduler { get; private set; }
	
	/// <summary>
	/// Executes tasks in the main thread, LateUpdate context.
	/// </summary>
	public static UnityTaskScheduler LateUpdateScheduler { get; private set; }

	/// <summary>
	/// Executes tasks in the main thread, FixedUpdate context.
	/// </summary>
	public static UnityTaskScheduler FixedUpdateScheduler { get; private set; }

	/// <summary>
	/// Executes tasks in the thread pool. It's an alias for TaskScheduler.Default.
	/// </summary>
	public static TaskScheduler ThreadPoolScheduler => TaskScheduler.Default;

	private void Awake()
	{
		if (Instance != null)
		{
			throw new NotSupportedException("UnityScheduler already exists.");
		}
		Instance = this;
		MainThreadId = Thread.CurrentThread.ManagedThreadId;

		DontDestroyOnLoad(gameObject);

		UpdateScheduler = new UnityTaskScheduler("Update");
		LateUpdateScheduler = new UnityTaskScheduler("LateUpdate");
		FixedUpdateScheduler = new UnityTaskScheduler("FixedUpdate");

		SynchronizationContext.SetSynchronizationContext(UpdateScheduler.Context);
	}

	private void Update() => UpdateScheduler.Activate();

	private void LateUpdate() => LateUpdateScheduler.Activate();

	private void FixedUpdate() => FixedUpdateScheduler.Activate();
}