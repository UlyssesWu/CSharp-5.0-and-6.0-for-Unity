using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

internal class ThreadPingPong : MonoBehaviour
{
	private void WhereAmI()
	{
		var threadId = Thread.CurrentThread.ManagedThreadId;
		Debug.Log(threadId == 1 ? "main thread" : "background thread #" + threadId);
	}

	public async void AsyncAwaitDemo()
	{
		WhereAmI(); // main thread

		var task1 = new Task(WhereAmI); // background thread
		task1.Start(TaskScheduler.Default);

		var task2 = new Task(WhereAmI); // main thread
		task2.Start(UnityScheduler.MainThread);

		var task3 = new Task(WhereAmI); // background thread
		task3.Start(TaskScheduler.Default);

		// returns execution of asynchronous method to the main thread,
		// if it was originally called from the main thread
		await TaskEx.WhenAll(task1, task2, task3);

		WhereAmI(); // main thread
		Debug.Log("done");
	}

	public void TaskContinueWithDemo()
	{
		WhereAmI(); // main thread

		var originalTask = new Task(WhereAmI); // background thread

		var continuationTask1 = originalTask.ContinueWith(
			previousTask => WhereAmI(),
			UnityScheduler.MainThread); // main thread

		var continuationTask2 = continuationTask1.ContinueWith(
			previousTask => WhereAmI()); // background thread

		var continuationTask3 = continuationTask2.ContinueWith(
			previousTask => WhereAmI(),
			UnityScheduler.MainThread); // main thread

		var continuationTask4 = continuationTask3.ContinueWith(
			previousTask => WhereAmI()); // background thread

		originalTask.Start(TaskScheduler.Default);
	}
}