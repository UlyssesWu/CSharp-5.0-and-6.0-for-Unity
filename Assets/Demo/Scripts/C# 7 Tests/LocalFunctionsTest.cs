using UnityEngine;

public class LocalFunctionsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Local Functions:");

        var suffix = "!";

        void print(int i) => Debug.Log("Printed from a local function: " + i + suffix);

        print(10);

        Debug.Log("");
    }
}