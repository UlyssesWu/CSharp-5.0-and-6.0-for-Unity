using UnityEngine;
using UnityEngine.UI;

internal class Logger : MonoBehaviour
{
    private Text textControl;

    private void Awake()
    {
        textControl = GetComponent<Text>();
        Application.logMessageReceivedThreaded += Application_logMessageReceived;
        Debug.Log("<color=red>Current platform: " + Application.platform + "</color>\n");
    }

    private void Application_logMessageReceived(string message, string stackTrace, LogType type)
    {
        textControl.text += message + "\n";
    }
}