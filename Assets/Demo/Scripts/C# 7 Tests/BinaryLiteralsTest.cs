using UnityEngine;

public class BinaryLiteralsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=yellow>Binary Literals:</color>");

        var number = 0b1110;
        Debug.Log($"bin 1110 = dec {number}");

        Debug.Log("");
    }
}