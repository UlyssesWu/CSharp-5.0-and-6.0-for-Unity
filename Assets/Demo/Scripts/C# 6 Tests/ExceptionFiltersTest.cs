using System;
using UnityEngine;

#warning IL2CPP may fail this test
internal class ExceptionFiltersTest : MonoBehaviour
{
	private class MyException : Exception
	{
		public int Severity { get; set; }
	}

	private void Start()
	{
        Debug.Log("<color=yellow>Exception Filters:</color>");

        try
        {
			throw new MyException { Severity = 3 };
		}
		catch (MyException ex) when (ex.Severity == 2)
		{
			Debug.Log("Will not execute");
		}
		catch (MyException ex) when (ex.Severity == 3)
		{
			Debug.Log("Will be executed");
		}

        Debug.Log("");
    }
}