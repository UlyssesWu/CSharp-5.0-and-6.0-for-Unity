using UnityEngine;

public class OutVariablesTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Out variables:");

        var s = "100";
        if (int.TryParse(s, out int i))
        {
            Debug.Log($"\"{s}\" => {i}");
        }

        Debug.Log("");
    }
}