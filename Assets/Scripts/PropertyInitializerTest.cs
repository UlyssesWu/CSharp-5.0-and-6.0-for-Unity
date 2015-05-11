using UnityEngine;

internal class PropertyInitializerTest : MonoBehaviour
{
	private string Property1 { get; } = "Hello, World! 111";
	private string Property2 { get; } = "Hello, World! 222";

	private void Start()
	{
		Debug.Log("Property1 value: " + Property1);
		Debug.Log("Property2 value: " + Property2);
	}
}