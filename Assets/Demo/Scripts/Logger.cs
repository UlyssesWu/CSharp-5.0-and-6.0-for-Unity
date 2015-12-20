using UnityEngine;
using UnityEngine.UI;

internal class Logger : MonoBehaviour
{
	private Text textControl;

	private void Awake()
	{
		textControl = GetComponent<Text>();
#if UNITY_5
		Application.logMessageReceivedThreaded += Application_logMessageReceived;
#else
		Application.RegisterLogCallback(Application_logMessageReceived);
#endif

		Debug.Log("Current platform: " + Application.platform);
	}

	private void Application_logMessageReceived(string message, string stackTrace, LogType type)
	{
		textControl.text += message + "\n";
	}
}