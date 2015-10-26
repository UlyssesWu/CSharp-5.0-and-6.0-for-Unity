using UnityEngine;

#warning Mono 4.0.3 compiler fails this test
internal class NullConditionalTest : MonoBehaviour
{
	private void Start()
	{
		var foo = new[] { "Aphrodite", "Apollo", "Ares", "Artemis", "Athena" };
		var bar = foo.Length != 0 ? new[] { 1, 2, 3, 4, 5 } : null;

		Debug.Log("Number of foos: " + foo?.Length);
		Debug.Log("3th bar has value: " + (bar?[3]).HasValue); // Mono 4.0.3 compiler fails here
		   // error CS1061: Type `int' does not contain a definition for `HasValue' and no extension method `HasValue'
		   // of type `int' could be found. Are you missing an assembly reference?
	}
}