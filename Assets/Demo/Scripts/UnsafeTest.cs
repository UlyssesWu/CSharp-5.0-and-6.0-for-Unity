using UnityEngine;

public class UnsafeTest : MonoBehaviour
{
	unsafe void Start()
	{
		var array = new[] { 1, 2, 3 };
		fixed (int* p = array)
		{
			var sum = *p + *(p + 1) + *(p + 2);
			Debug.Log("Unsafe result: " + sum);
		}
	}
}
