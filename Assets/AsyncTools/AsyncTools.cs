using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class AsyncTools
{
	private static readonly Awaiter mainThreadAwaiter = new SynchronizationContextAwaiter(UnityScheduler.SynchronizationContext);
	private static readonly Awaiter threadPoolAwaiter = new ThreadPoolContextAwaiter();
	private static readonly Awaiter doNothingAwaiter = new DoNothingAwaiter();

	public static void WhereAmI(string text)
	{
		if (IsMainThread())
		{
			Debug.Log($"{text}: main thread, frame: {Time.frameCount}");
		}
		else
		{
			Debug.Log($"{text}: background thread, id: {Thread.CurrentThread.ManagedThreadId}");
		}
	}

	public static bool IsMainThread()
	{
		return Thread.CurrentThread.ManagedThreadId == UnityScheduler.MainThreadId;
	}

	public static Awaiter ToThreadPool()
	{
		return IsMainThread() ? threadPoolAwaiter : doNothingAwaiter;
	}

	public static Awaiter ToMainThread()
	{
		return IsMainThread() ? doNothingAwaiter : mainThreadAwaiter;
	}

	public static Task<byte[]> DownloadAsBytesAsync(string address)
	{
		var task = new Task<byte[]>(
			() =>
			{
				using (var webClient = new WebClient())
				{
					return webClient.DownloadData(address);
				}
			});
		task.Start(UnityScheduler.ThreadPoolScheduler);
		return task;
	}

	public static Task<string> DownloadAsStringAsync(string address)
	{
		var task = new Task<string>(
			() =>
			{
				using (var webClient = new WebClient())
				{
					return webClient.DownloadString(address);
				}
			});
		task.Start(UnityScheduler.ThreadPoolScheduler);
		return task;
	}

	public static TaskAwaiter GetAwaiter(this float seconds)
	{
		seconds = Math.Max(seconds, .001f); // makes 'await 0f' an equivalent of Unity's 'yield return null'
		return TaskEx.Delay((int)(seconds * 1000)).GetAwaiter();
	}

	public static TaskAwaiter GetAwaiter(this int seconds)
	{
		return GetAwaiter((float)seconds);
	}

	/// <summary>
	/// Waits until all the tasks are completed
	/// </summary>
	public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks)
	{
		return TaskEx.WhenAll(tasks).GetAwaiter();
	}

	/// <summary>
	/// Waits until the process exits
	/// </summary>
	public static TaskAwaiter<int> GetAwaiter(this Process process)
	{
		var tcs = new TaskCompletionSource<int>();
		process.EnableRaisingEvents = true;
		process.Exited += (sender, eventArgs) => tcs.TrySetResult(process.ExitCode);
		if (process.HasExited)
		{
			tcs.TrySetResult(process.ExitCode);
		}
		return tcs.Task.GetAwaiter();
	}

	#region Context switching awaiter classes

	public abstract class Awaiter : INotifyCompletion
	{
		public abstract void OnCompleted(Action continuation);
		public Awaiter GetAwaiter() => this;
		public abstract bool IsCompleted { get; }
		public void GetResult() { }
	}

	private class DoNothingAwaiter : Awaiter
	{
		public override bool IsCompleted => true;
		public override void OnCompleted(Action action) => action();
	}

	private class SynchronizationContextAwaiter : Awaiter
	{
		private readonly SynchronizationContext context;

		public SynchronizationContextAwaiter(SynchronizationContext context)
		{
			this.context = context;
		}

		public override bool IsCompleted => false;
		public override void OnCompleted(Action action) => context.Post(state => action(), null);
	}

	private class ThreadPoolContextAwaiter : Awaiter
	{
		public override bool IsCompleted => false;
		public override void OnCompleted(Action action) => ThreadPool.QueueUserWorkItem(state => action(), null);
	}

	#endregion
}