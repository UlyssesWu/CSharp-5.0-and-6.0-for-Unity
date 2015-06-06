using System.Runtime.CompilerServices;
using UnityEngine;

internal class CallerInfoTest : MonoBehaviour
{
	private void Start()
	{
		CallMe();
	}

	private void CallMe([CallerFilePath] string path = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Debug.Log("Caller path: " + path);
		Debug.Log("Calling member: " + member);
		Debug.Log("Caller line: " + line);
	}
}