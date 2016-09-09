using UnityEngine;

internal class NameofTest : MonoBehaviour
{
	private void Start()
	{
        Debug.Log("<color=yellow>Nameof:</color>");

        Debug.Log("This script's name is: " + nameof(NameofTest));

        Debug.Log("");
    }
}