using System;
using UnityEngine;

internal class ExpressionBodiedMembersTest : MonoBehaviour
{
	private void Print(string message) => Debug.Log(message);

	private string DayOfWeekPlusTen => DateTime.Now.DayOfWeek.ToString() + 10;

	private void Start()
	{
		Print("Day of week plus ten: " + DayOfWeekPlusTen);
	}
}