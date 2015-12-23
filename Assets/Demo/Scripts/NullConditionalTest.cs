using UnityEngine;

internal class NullConditionalTest : MonoBehaviour
{
	private void Start()
	{
		var foo = new[] { "Aphrodite", "Apollo", "Ares", "Artemis", "Athena" };
		var bar = foo.Length != 0 ? new[] { 1, 2, 3, 4, 5 } : null;

		Debug.Log("Number of foos: " + foo?.Length);

#warning Mono C# compiler version 4.2.1.0 fails this test
		/*
		Debug.Log("3th bar has value: " + (bar?[3]).HasValue); // Mono C# compiler fails here, throwing
		   // error CS1061: Type `int' does not contain a definition for `HasValue' and no extension method `HasValue'
		   // of type `int' could be found. Are you missing an assembly reference?
		*/
	}
}