using UnityEngine;

public class DigitSeparatorsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Digit Separators:");

        var number = 10_000_000_000;
        Debug.Log($"10 billions = {number:N0}");

        Debug.Log("");
    }
}