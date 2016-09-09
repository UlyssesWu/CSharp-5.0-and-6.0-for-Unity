using System;
using UnityEngine;

internal class ExpressionBodiedMembersTest : MonoBehaviour
{
	private void Print(string message) => Debug.Log(message);

	private string DayOfWeekPlusTen => DateTime.Now.DayOfWeek.ToString() + 10;

	private void Start()
	{
        Debug.Log("<color=yellow>Expression Bodied Members:</color>");

        Print("Day of week plus ten: " + DayOfWeekPlusTen);

        Debug.Log("");
    }
}