using UnityEngine;

internal class PropertyInitializerTest : MonoBehaviour
{
	private string Property1 { get; } = "Hello, World! 111";
	private string Property2 { get; set; } = "Hello, World! 222";

	private void Start()
	{
		Debug.Log("Property1 value: " + Property1);
		Debug.Log("Property2 value: " + Property2);
		Property2 = "Bye, World!";
	}
}

// Mono C# compiler version 4.2.1.0 passes this test:
internal class Abc { }

internal class Test
{
	public Abc Abc { get; }

	public Test()
	{
		Abc = new Abc(); // Earlier versions of Mono C# compiler were failing here, throwing
						 // error CS0118: `Abc' is a `type' but a `variable' was expected
	}
}
