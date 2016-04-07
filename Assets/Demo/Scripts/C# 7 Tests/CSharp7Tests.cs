using System.Collections.Generic;
using UnityEngine;

public class CSharp7Tests : MonoBehaviour
{
    private void Start()
    {
        var someCoeff = 2;

        // Local functions
        int MultiplyByCoeff(int x) => someCoeff * x;
        Debug.Log($"coeff = {someCoeff}, MultiplyByCoeff(10) = {MultiplyByCoeff(10)}");

        // Binary literals
        var bin = 0b1110;
        Debug.Log($"bin 1110 = dec {bin}");

        // Digit separators
        var number = 10_000_000_000;
        Debug.Log($"10 billions = {number}");

        // Pattern matching
        int? nullable = 10;
        Debug.Log(nullable is int i ? $"not null: {i}" : "is null");

        Match(1);
        Match(6);
        Match(3d);
        Match(8f);
        Match(12f);
        Match("hi");
        Match("hello world");
        Match(new KeyValuePair<int, string>(4, "melon"));
        Match(new KeyValuePair<int, string>(6, "apple"));
        Match(new KeyValuePair<int, string>(8, "orange"));
        Match(new GameObject("object"));

        // Ref-returns and ref-locals
        int a = 1;
        int b = 4;
        ref int max = ref GetMax(ref a, ref b);
        Debug.Log($"a = {a}, b = {b}");
        max = 100;
        Debug.Log($"a = {a}, b = {b}");
    }

    private ref int GetMax(ref int a, ref int b)
    {
        if (a > b)
        {
            return ref a;
        }
        return ref b;
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
        }

        let reference = 6; // readonly local

        switch (o)
        {
            case 1: // Constant
                Debug.Log($"o is int = one");
                break;

            case reference: // Variable
                Debug.Log($"o is int = reference: {o}={reference}");
                break;

            case int d: // Type
                Debug.Log($"o is int: {d}");
                break;

            case KeyValuePair<int, string>(*, "melon"):
                Debug.Log($"o is {{*, melon}}");
                break;

            case KeyValuePair<int, string>(var key, *) when key == 6:
                Debug.Log($"o is {{key=six, *}}");
                break;

            case KeyValuePair<int, string> { Key is var key, Value is var value }:
                Debug.Log($"o is {{key={key}, value={value}}}");
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
