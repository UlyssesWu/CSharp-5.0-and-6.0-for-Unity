#define LOGGING_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

internal class Program
{
	private enum CompilerVersion
	{
		Version3,
		Version5,
		Version6Microsoft,
		Version6Mono
	}

	private const string LOG_FILENAME = "compilation log.txt";

	private static readonly List<string> OutputLines = new List<string>();
	private static readonly List<string> ErrorLines = new List<string>();

	private static int Main(string[] args)
	{
		try
		{
			var compilationOptions = GetCompilationOptions(args);
			var unityEditorDataDir = GetUnityEditorDataDir(compilationOptions);

			InitLog();
			Log($"smcs.exe version: {Assembly.GetExecutingAssembly().GetName().Version}");
			Log($"Project directory: {Directory.GetCurrentDirectory()}");
			Log($"Unity directory: {unityEditorDataDir}");

			CompilerVersion compilerVersion;
			if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Roslyn")))
			{
				compilerVersion = CompilerVersion.Version6Microsoft;
			}
			else if (File.Exists("mcs.exe"))
			{
				compilerVersion = CompilerVersion.Version6Mono;
			}
			else if (compilationOptions.Any(line => line.Contains("AsyncBridge.Net35.dll")))
			{
				compilerVersion = CompilerVersion.Version5;
			}
			else
			{
				compilerVersion = CompilerVersion.Version3;
			}
			Log($"Compiler: {compilerVersion}");
			Log("\n- Compilation -----------------------------------------------\n");

			var stopwatch = Stopwatch.StartNew();
			var process = CreateCompilerProcess(compilerVersion, unityEditorDataDir, args[0]);

			Log($"Process: {process.StartInfo.FileName}");
			Log($"Arguments: {process.StartInfo.Arguments}");

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();

			stopwatch.Stop();
			Log($"Exit code: {process.ExitCode}");
			Log($"Elapsed time: {stopwatch.ElapsedMilliseconds / 1000f:F2} sec");

			if (compilerVersion == CompilerVersion.Version6Microsoft)
			{
				// Since Microsoft's compiler writes all warnings and errors to the standard output channel,
				// move them to the error channel

				while (OutputLines.Count > 4)
				{
					var line = OutputLines[3];
					OutputLines.RemoveAt(3);
					ErrorLines.Add(line);
				}
			}

			Log("\n- Compiler output:");
			for (int i = 0; i < OutputLines.Count; i++)
			{
				var line = OutputLines[i];
				Console.Out.WriteLine(line);
				Log($"{i}: {line}");
			}

			Log("\n- Compiler errors:");
			for (int i = 0; i < ErrorLines.Count; i++)
			{
				var line = ErrorLines[i];
				Console.Error.WriteLine(line);
				Log($"{i}: {line}");
			}

			if (process.ExitCode != 0 || compilerVersion != CompilerVersion.Version6Microsoft)
			{
				return process.ExitCode;
			}

			Log("\n- PDB to MDB conversion --------------------------------------\n");

			OutputLines.Clear();
			ErrorLines.Clear();

			var pdb2mdbPath = Path.Combine(Directory.GetCurrentDirectory(), @"Roslyn/pdb2mdb.exe");
			var libraryPath = Directory.GetFiles("Temp", "*.dll").First();
			var pdbPath = Path.Combine("Temp", Path.GetFileNameWithoutExtension(Directory.GetFiles("Temp", "*.dll").First()) + ".pdb");

			process = new Process
			{
				StartInfo =
						  {
							  FileName = pdb2mdbPath,
							  Arguments = libraryPath,
							  UseShellExecute = false,
							  RedirectStandardOutput = true,
							  CreateNoWindow = true,
						  }
			};

			process.OutputDataReceived += Process_OutputDataReceived;

			Log($"Process: {process.StartInfo.FileName}");
			Log($"Arguments: {process.StartInfo.Arguments}");

			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();

			File.Delete(pdbPath);

			Log("\n- pdb2mdb.exe output:");
			foreach (var line in OutputLines)
			{
				Log(line);
			}
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.Write($"Compiler redirection error: {e.Message}\n{e.StackTrace}");
			return -1;
		}
	}

	private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		OutputLines.Add(e.Data);
	}

	private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		ErrorLines.Add(e.Data);
	}

	[Conditional("LOGGING_ENABLED")]
	private static void InitLog()
	{
		var dateTimeString = DateTime.Now.ToString("F");
		var middleLine = "*" + new string(' ', 78) + "*";
		int index = (80 - dateTimeString.Length) / 2;
		middleLine = middleLine.Remove(index, dateTimeString.Length).Insert(index, dateTimeString);

		string header = new string('*', 80) + "\n";
		header += middleLine + "\n";
		header += new string('*', 80) + "\n\n";

		if (File.Exists(LOG_FILENAME))
		{
			var lastWriteTime = new FileInfo(LOG_FILENAME).LastWriteTimeUtc;
			if (DateTime.UtcNow - lastWriteTime > TimeSpan.FromMinutes(5))
			{
				File.WriteAllText(LOG_FILENAME, header);
			}
			else
			{
				File.AppendAllText(LOG_FILENAME, header);
			}
		}
		else
		{
			File.WriteAllText(LOG_FILENAME, header);
		}
	}

	[Conditional("LOGGING_ENABLED")]
	private static void Log(string message)
	{
		File.AppendAllText(LOG_FILENAME, message + "\n");
	}

	private static Process CreateCompilerProcess(CompilerVersion version, string unityEditorDataDir, string responseFile)
	{
		string processPath;
		string processArguments;

		var systemDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.dll");
		var systemCoreDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Core.dll");
		var systemXmlDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Xml.dll");

		switch (version)
		{
			case CompilerVersion.Version3:
				processPath = Path.Combine(unityEditorDataDir, @"Mono/bin/mono.exe");
				var compilerPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/gmcs.exe");
				processArguments = $"\"{compilerPath}\" {responseFile}";
				break;

			case CompilerVersion.Version5:
				processPath = Path.Combine(unityEditorDataDir, @"MonoBleedingEdge/lib/mono/4.5/mcs.exe");
				processArguments = $"-sdk:2 -langversion:Future -r:\"{systemCoreDllPath}\" {responseFile}";
				break;

			case CompilerVersion.Version6Mono:
				processPath = Path.Combine(Directory.GetCurrentDirectory(), "mcs.exe");
				processArguments = $"-sdk:2 -r:\"{systemCoreDllPath}\" {responseFile}";
				break;

			case CompilerVersion.Version6Microsoft:
				processPath = Path.Combine(Directory.GetCurrentDirectory(), @"Roslyn/csc.exe");
				var mscorlib = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/mscorlib.dll");
				processArguments =
					$"-nostdlib+ -noconfig -r:\"{mscorlib}\" -r:\"{systemDllPath}\" -r:\"{systemCoreDllPath}\" -r:\"{systemXmlDllPath}\" {responseFile}";
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(version), version, null);
		}

		var process = new Process
		{
			StartInfo =
						  {
							  FileName = processPath,
							  Arguments = processArguments,
							  RedirectStandardError = true,
							  RedirectStandardOutput = true,
							  UseShellExecute = false
						  }
		};

		process.OutputDataReceived += Process_OutputDataReceived;
		process.ErrorDataReceived += Process_ErrorDataReceived;

		return process;
	}

	private static string GetUnityEditorDataDir(string[] compilationOptions)
	{
		var filename = compilationOptions.First(line => line.Contains("UnityEngine.dll")).Substring(3).Trim('\"');
		var index = filename.IndexOf("Data");
		filename = filename.Substring(0, index + "Data".Length);
		return filename;
	}

	private static string[] GetCompilationOptions(string[] args)
	{
		var compilationOptions = File.ReadAllLines(args[0].TrimStart('@'));
		return compilationOptions;
	}
}