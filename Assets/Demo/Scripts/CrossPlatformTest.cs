using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CrossPlatformTest : MonoBehaviour
{
	private void Start()
	{
		Type type = null;
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			type = assembly.GetType("System.Threading.Platform");
			if (type != null)
			{
				break;
			}
		}
		var methodInfo = type.GetMethod("Yield", BindingFlags.Static | BindingFlags.NonPublic);

		ShowResult(false);

		methodInfo.Invoke(null, null);

		ShowResult(true);
	}

	private static void ShowResult(bool success)
	{
		Camera.main.backgroundColor = success ? Color.green : Color.magenta;
		var textObject = FindObjectOfType<Text>();
		textObject.text = success ? "SUCCESS" : "FAIL";
	}
}