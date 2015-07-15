using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class AsyncTools
{
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
		task.Start(TaskScheduler.Default);
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
		task.Start(TaskScheduler.Default);
		return task;
	}

	public static TaskAwaiter GetAwaiter(this float seconds)
	{
		return TaskEx.Delay((int)(seconds * 1000)).GetAwaiter();
	}

	public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks)
	{
		return TaskEx.WhenAll(tasks).GetAwaiter();
	}

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
}