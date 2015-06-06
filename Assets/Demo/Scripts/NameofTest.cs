using UnityEngine;

internal class NameofTest : MonoBehaviour
{
	private void Start()
	{
		Debug.Log("This script's name is: " + nameof(NameofTest));
	}
}