using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

internal class UnityTaskSchedulerDemo : MonoBehaviour
{
	private void Start()
	{
		Register("email", "password");
	}

	public void Register(string email, string password)
	{
		try
		{
			var user = new ParseUser { Username = email, Email = email, Password = password };
			user.SignUpAsync().ContinueWith(
				t =>
				{
					if (t.IsFaulted || t.IsCanceled)
					{
						WhereAmI();
						Debug.Log("failed or canceled");
					}
					else
					{
						WhereAmI();
						Debug.Log("succeded");
					}
				},
				UnityScheduler.MainThread);
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}
	}

	private void WhereAmI()
	{
		Debug.Log("");
		Debug.Log("thread id: " + Thread.CurrentThread.ManagedThreadId);
	}
}

public class ParseUser
{
	public string Username { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }

	public Task SignUpAsync()
	{
		var task = new Task(
			() =>
			{
				Thread.Sleep(3000);
			});
		task.Start(TaskScheduler.Default);

		return task;
	}
}