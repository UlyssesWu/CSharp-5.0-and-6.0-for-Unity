using System;
using UnityEngine;

internal class StringInterpolationTest : MonoBehaviour
{
    private void Start()
    {
        var x = 100;
        var y = 200;

        Debug.Log($"x = {x}, y = {y}");
        Debug.Log($"x > y = {x > y}");

        var id = 100;
        var name = "%Alice&";
        string url = Url($"http://foobar/item/{id}/{name}");
        Debug.Log(url);
    }

    private static string Url(FormattableString formattable)
    {
        return formattable.ToString(new UrlFormatProvider());
    }
}

internal class UrlFormatProvider : IFormatProvider
{
    private readonly UrlFormatter formatter = new UrlFormatter();

    public object GetFormat(Type formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return formatter;
        }
        return null;
    }

    private class UrlFormatter : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return string.Empty;
            }
            if (format == "r")
            {
                return arg.ToString();
            }
            return Uri.EscapeDataString(arg.ToString());
        }
    }
}