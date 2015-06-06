using System;
using UnityEngine;

internal class ExeptionFiltersTest : MonoBehaviour
{
	private class MyException : Exception
	{
		public int Severity { get; set; }
	}

	private void Start()
	{
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
	}
}