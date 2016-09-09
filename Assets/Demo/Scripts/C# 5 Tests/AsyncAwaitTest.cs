using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncAwaitTest : MonoBehaviour
{
	private async void Start()
	{
        await TaskEx.Delay(500);
        Debug.Log("<color=yellow>Async/Await:</color>");

        PrintThreadId();

		for (int i = 0; i < 5; i++)
		{
			await TaskEx.Delay(200);
			PrintThreadId();
		}

		var tasks = new List<Task>();
		for (int i = 0; i < 5; i++)
		{
			tasks.Add(new Task(PrintThreadId));
		}
		Parallel.ForEach(tasks, t => t.RunSynchronously());

		await TaskEx.WhenAll(tasks);

		PrintThreadId();
		Debug.Log("Finish");

        Debug.Log("");
    }

    private static void PrintThreadId()
	{
		Debug.Log("thread id: " + Thread.CurrentThread.ManagedThreadId);
	}

	private void Update()
	{
		transform.Rotate(0, 90 * Time.deltaTime, 0);
	}
}