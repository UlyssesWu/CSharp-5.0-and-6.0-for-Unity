using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncTest : MonoBehaviour
{
	private async void Start()
	{
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

		var x = 100;
		Debug.Log($"x={x}");
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