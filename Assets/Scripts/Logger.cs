using UnityEngine;
using UnityEngine.UI;

internal class Logger : MonoBehaviour
{
	private Text textControl;

	private void Awake()
	{
		textControl = GetComponent<Text>();
		Application.logMessageReceivedThreaded += Application_logMessageReceived;

		Debug.Log("Current platform: " + Application.platform);
	}

	private void Application_logMessageReceived(string message, string stackTrace, LogType type)
	{
		textControl.text += message + "\n";
	}
}