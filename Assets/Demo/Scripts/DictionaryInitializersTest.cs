using System.Collections.Generic;
using UnityEngine;

internal class DictionaryInitializersTest : MonoBehaviour
{
	private void Start()
	{
		var table = new Dictionary<int, string>
					{
						[0] = "cero",
						[1] = "uno",
						[2] = "dos",
						[3] = "tres",
					};
		Debug.Log("2: " + table[2]);
	}
}