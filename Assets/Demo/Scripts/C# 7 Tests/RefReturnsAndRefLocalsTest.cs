using UnityEngine;

public class RefReturnsAndRefLocalsTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=yellow>Ref Returns and Ref Locals:</color>");

        int a = 1;
        int b = 4;
        Debug.Log($"a = {a}, b = {b}");

        GetMax(ref a, ref b) = 100;
        Debug.Log($"a = {a}, b = {b}");

        // transform.position.y = 200;  <-- illegal
        // (Error CS1612: Cannot modify the return value of 'Transform.position' because it is not a variable)
        
        var myTransform = new MyTransform();

        myTransform.position = new Vector3(1, 2, 3);
        Debug.Log(myTransform.position);

        myTransform.position.y = 200; // <-- legal
        Debug.Log(myTransform.position);

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

public class MyTransform
{
    Vector3 internalPosition;

    public ref Vector3 position
    {
        get { return ref internalPosition; }
    }
}