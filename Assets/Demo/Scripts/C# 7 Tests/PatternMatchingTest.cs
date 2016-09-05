using UnityEngine;

public class PatternMatchingTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Pattern matching:");

        int? nullable = 10;
        Debug.Log(nullable is int i ? $"not null: {i}" : "is null");

        Match(1);
        Match(3d);
        Match(8f);
        Match(12f);
        Match("hi");
        Match("hello world");
        Match(new GameObject("object"));

        Debug.Log("");
    }

    private void Match(object o)
    {
        // Pattern matching
        if (o is GameObject go)
        {
            Debug.Log("o is GameObject");
            return;
        }

        if (o is string s && s.Length > 5)
        {
            Debug.Log($"o is long string: {s}");
            return;
        }

        switch (o)
        {
            case 1: // Constant
                Debug.Log($"o is int = one");
                break;

            case int d: // Type
                Debug.Log($"o is int: {d}");
                break;

            case float f when f > 10:
                Debug.Log($"o is float greater than 10: {f}");
                break;

            case float f:
                Debug.Log($"o is float: {f}");
                break;

            default:
                Debug.Log("o is some object");
                break;
        }
    }
}