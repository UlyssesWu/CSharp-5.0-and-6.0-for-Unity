using UnityEngine;

public class LocalFunctionsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=yellow>Local Functions:</color>");

        var suffix = "!";

        void print(int i) => Debug.Log("Printed from a local function: " + i + suffix);

        print(10);

        Debug.Log("");
    }
}