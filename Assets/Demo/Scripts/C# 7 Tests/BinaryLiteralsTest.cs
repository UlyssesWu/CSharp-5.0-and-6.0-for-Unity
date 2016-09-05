using UnityEngine;

public class BinaryLiteralsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Binary Literals:");

        var number = 0b1110;
        Debug.Log($"bin 1110 = dec {number}");

        Debug.Log("");
    }
}