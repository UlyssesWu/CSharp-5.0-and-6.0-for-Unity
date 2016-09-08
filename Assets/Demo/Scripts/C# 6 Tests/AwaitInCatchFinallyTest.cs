using System;
using System.Threading.Tasks;
using UnityEngine;

internal class AwaitInCatchFinallyTest : MonoBehaviour
{
	private async void Start()
	{
        await TaskEx.Delay(500);
        Debug.Log("<color=yellow>Await in catch/finally:</color>");

        try
        {
			throw new NotImplementedException();
		}
		catch (NotImplementedException)
		{
			await TaskEx.Delay(100);
			Debug.Log("Inside 'catch'");
		}
		finally
		{
			await TaskEx.Delay(100);
			Debug.Log("Inside 'finally'");
		}

        Debug.Log("");
    }
}