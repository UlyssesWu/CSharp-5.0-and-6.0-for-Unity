using UnityEngine;

internal class StringInterpolationTest : MonoBehaviour
{
	private void Start()
	{
		var x = 100;
		var y = 200;

		Debug.Log($"x = {x}, y = {y}");
		Debug.Log($"x > y = {x > y}");
	}
}