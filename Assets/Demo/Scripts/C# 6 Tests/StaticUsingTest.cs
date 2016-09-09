using UnityEngine;
using static UnityEngine.Vector2;
using static UnityEngine.Debug;

internal class StaticUsingTest : MonoBehaviour
{
	private void Start()
	{
        Log("<color=yellow>Static Using:</color>");

        var point = new Vector2(3, 4);
		Log("Distance from (0,0) to (3,4) = " + Distance(zero, point));

        Log("");
    }
}