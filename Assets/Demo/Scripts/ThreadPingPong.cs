using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

internal class ThreadPingPong : MonoBehaviour
{
	public async void AsyncAwaitDemo()
	{
		AsyncTools.WhereAmI("1"); // main thread

		var task1 = Task.Factory.StartNew(() => AsyncTools.WhereAmI("2")); // background thread

		var task2 = new Task(() => AsyncTools.WhereAmI("3")); // main thread
		task2.Start(UnityScheduler.MainThreadScheduler);

		var task3 = Task.Factory.StartNew(() => AsyncTools.WhereAmI("5")); // background thread

		// returns execution of asynchronous method to the main thread,
		// if it was originally called from the main thread
		await TaskEx.WhenAll(task1, task2, task3);
		AsyncTools.WhereAmI("5"); // main thread

		await TaskEx.Delay(100).ConfigureAwait(false);
		AsyncTools.WhereAmI("6"); // can be any thread, since the previous line states that we don't care

		Debug.Log("done");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public async void TaskContinueWithDemo()
	{
		AsyncTools.WhereAmI("1"); // main thread

		var originalTask = new Task(() => AsyncTools.WhereAmI("2"));

		var continuationTask1 = originalTask.ContinueWith(
			previousTask => AsyncTools.WhereAmI("3"),
			UnityScheduler.MainThreadScheduler); // main thread

		var continuationTask2 = continuationTask1.ContinueWith(
			previousTask => AsyncTools.WhereAmI("4")); // background thread

		var continuationTask3 = continuationTask2.ContinueWith(
			previousTask => AsyncTools.WhereAmI("5"),
			UnityScheduler.MainThreadScheduler); // main thread

		var continuationTask4 = continuationTask3.ContinueWith(
			previousTask => AsyncTools.WhereAmI("6")); // background thread

		originalTask.Start(UnityScheduler.ThreadPoolScheduler);  // start the task chain from a background thread

		await continuationTask4;
		Debug.Log("done");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void TaskRunSynchronouslyMainThreadDemo()
	{
		Debug.Log("Launched from the main thread...");
		RunTasksSynchronously();
		Debug.Log("done");
	}

	public async void TaskRunSynchronouslyBackgroundThreadDemo()
	{
		Debug.Log("Launched from a background thread...");

		await Task.Factory.StartNew(RunTasksSynchronously);
		Debug.Log("done");
	}

	private void RunTasksSynchronously()
	{
		/*
		Fact #1: ThreadPoolScheduler supports running tasks on any thread.
		Fact #2: MainThreadScheduler supports running tasks on the main thread only.

		If this method is called from the main thread, all the tasks will be executed on the main thread.
		
		If this method is called from a background thread, the tasks associated with the thread pool scheduler
		will be executed on the current background thread, and the tasks associated with the main thread scheduler will be
		executed on the main thread and the current background thread will be blocked until the execution completes.
		*/

		AsyncTools.WhereAmI("1");

		var task = new Task(() => AsyncTools.WhereAmI("2"));
		task.RunSynchronously(UnityScheduler.MainThreadScheduler);

		task = new Task(() => AsyncTools.WhereAmI("3"));
		task.RunSynchronously(UnityScheduler.ThreadPoolScheduler);

		task = new Task(() => AsyncTools.WhereAmI("4"));
		task.RunSynchronously(UnityScheduler.MainThreadScheduler);

		task = new Task(() => AsyncTools.WhereAmI("5"));
		task.RunSynchronously(); // no scheduler => use default, which is ThreadPoolScheduler 

		task = new Task(() => AsyncTools.WhereAmI("6"));
		task.RunSynchronously(UnityScheduler.MainThreadScheduler);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public async void ContextSwitchMainThreadDemo()
	{
		Debug.Log("Launched from the main thread...");
		await TestContextSwitch();
		Debug.Log("done");
	}

	public async void ContextSwitchBackgroundThreadDemo()
	{
		Debug.Log("Launched from a background thread...");
		await Task.Factory.StartNew(async () => await TestContextSwitch()).Unwrap();
		Debug.Log("done");
	}

	private static async Task TestContextSwitch()
	{
		AsyncTools.WhereAmI("1");

		await AsyncTools.ToThreadPool();
		AsyncTools.WhereAmI("2");

		await AsyncTools.ToMainThread();
		AsyncTools.WhereAmI("3");

		await AsyncTools.ToThreadPool();
		AsyncTools.WhereAmI("4");

		await AsyncTools.ToMainThread();
		AsyncTools.WhereAmI("5");
	}
}