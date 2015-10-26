﻿using System;
using System.IO;
using System.Text;
using System.Threading;

internal class Logger : IDisposable
{
	private enum LogginStyle
	{
		Immediate,
		Retained,
	}

	private const string LOG_FILENAME = "compilation log.txt";
	private const int MAXIMUM_FILE_AGE_IN_MINUTES = 5;

	private readonly Mutex mutex;
	private readonly LogginStyle logginStyle;
	private readonly StringBuilder pendingLines = new StringBuilder();

	public Logger()
	{
		mutex = new Mutex(true, "smcs");

		if (mutex.WaitOne(0)) // check if no other process is owning the mutex
		{
			logginStyle = LogginStyle.Immediate;
			DeleteLogFileIfTooOld();
		}
		else
		{
			logginStyle = LogginStyle.Retained;
		}
	}

	public void Dispose()
	{
		mutex.WaitOne(); // make sure we own the mutex now, so no other process is writing to the file

		if (logginStyle == LogginStyle.Retained)
		{
			DeleteLogFileIfTooOld();
			File.AppendAllText(LOG_FILENAME, pendingLines.ToString());
		}

		mutex.ReleaseMutex();
	}

	private void DeleteLogFileIfTooOld()
	{
		var lastWriteTime = new FileInfo(LOG_FILENAME).LastWriteTimeUtc;
		if (DateTime.UtcNow - lastWriteTime > TimeSpan.FromMinutes(MAXIMUM_FILE_AGE_IN_MINUTES))
		{
			File.Delete(LOG_FILENAME);
		}
	}

	public void AppendHeader()
	{
		var dateTimeString = DateTime.Now.ToString("F");
		var middleLine = "*" + new string(' ', 78) + "*";
		int index = (80 - dateTimeString.Length) / 2;
		middleLine = middleLine.Remove(index, dateTimeString.Length).Insert(index, dateTimeString);

		Append(new string('*', 80));
		Append(middleLine);
		Append(new string('*', 80));
	}

	public void Append(string message)
	{
		if (logginStyle == LogginStyle.Immediate)
		{
			File.AppendAllText(LOG_FILENAME, message + Environment.NewLine);
		}
		else
		{
			pendingLines.AppendLine(message);
		}
	}
}