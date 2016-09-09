using UnityEngine;

public class TuplesTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=yellow>Tuples and Deconstruction</color>:");

        var i = 10;
        var tuple = GetTuple(i);

        Debug.Log($"{i} doubled = {tuple.doubled}, {i} squared = {tuple.squared}, text = {tuple.text}");

        // Deconstruction
        // #1
        (int first, int second, string third) = tuple;
        Debug.Log($"first = {first}, second = {second}, third = {third}");

        // #2
        var (primero, segundo, tercero) = tuple;
        Debug.Log($"primero = {primero}, segundo = {segundo}, tercero = {tercero}");

        // #3
        int a, b = 42;
        string text = "some text";
        (a, b, text) = tuple;
        Debug.Log($"a = {a}, b = {b}, text = {text}");

        // #4
        var vector = new Vector3(1, 2.5f, 3);
        var (x, y, z) = vector;
        Debug.Log($"Vector {vector} => x = {x}, y = {y}, z = {z}");

        Debug.Log("");
    }

    (int doubled, int squared, string text) GetTuple(int x) => (x * 2, x * x, x.ToString() + "!");
}

static class Vector3Extensions
{
    public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}