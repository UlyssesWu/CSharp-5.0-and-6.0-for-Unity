using UnityEngine;

public class RefReturnsAndRefLocalsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Ref returns and ref locals:");

        int a = 1;
        int b = 4;
        ref int max = ref GetMax(ref a, ref b);
        Debug.Log($"a = {a}, b = {b}");
        max = 100;
        Debug.Log($"a = {a}, b = {b}");

        Debug.Log("");
    }

    private ref int GetMax(ref int a, ref int b)
    {
        if (a > b)
        {
            return ref a;
        }
        return ref b;
    }
}