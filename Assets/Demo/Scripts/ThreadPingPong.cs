using System.Threading.Tasks;
using UnityEngine;

internal class ThreadPingPong : MonoBehaviour
{
	public async void AsyncAwaitDemo()
	{
		AsyncTools.WhereAmI("1"); // main thread

		var task1 = new Task(() => AsyncTools.WhereAmI("2")); // background thread
		task1.Start(UnityScheduler.ThreadPoolScheduler);

		var task2 = new Task(() => AsyncTools.WhereAmI("3")); // main thread
		task2.Start(UnityScheduler.MainThreadScheduler);

		var task3 = new Task(() => AsyncTools.WhereAmI("4")); // background thread
		task3.Start(UnityScheduler.ThreadPoolScheduler);

		// returns execution of asynchronous method to the main thread,
		// if it was originally called from the main thread
		await TaskEx.WhenAll(task1, task2, task3);

		AsyncTools.WhereAmI("5"); // main thread
		Debug.Log("done");
	}

	public async void TaskContinueWithDemo()
	{
		AsyncTools.WhereAmI("1"); // main thread

		var originalTask = new Task(() => AsyncTools.WhereAmI("2")); // background thread

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

		originalTask.Start(UnityScheduler.ThreadPoolScheduler);
	}
}