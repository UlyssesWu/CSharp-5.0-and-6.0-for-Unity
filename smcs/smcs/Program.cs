#define LOGGING_ENABLED

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

internal class Program
{
	private enum CompilerVersion
	{
		Version3,
		Version5,
		Version6Microsoft,
		Version6Mono,
	}

	private const string LOG_FILENAME = "compilation log.txt";

	private static int Main(string[] args)
	{
		try
		{
			var compilationOptions = GetCompilationOptions(args);
			var unityEditorDataDir = GetUnityEditorDataDir(compilationOptions);

			InitLog();
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
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.Start();

			Log($"Process: {process.StartInfo.FileName}");
			Log($"Arguments: {process.StartInfo.Arguments}");

			string sourceOutput = process.StandardOutput.ReadToEnd();
			string sourceError = process.StandardError.ReadToEnd();
			process.WaitForExit();
			stopwatch.Stop();
			Log($"Exit code: {process.ExitCode}");
			Log($"Elapsed time: {stopwatch.ElapsedMilliseconds/1000f:F2} sec");

			var outputLines = sourceOutput.Replace("\r\n", "\n").Split('\n').ToList();
			var errorLines = sourceError.Replace("\r\n", "\n").Split('\n').ToList();

			if (compilerVersion == CompilerVersion.Version6Microsoft)
			{
				// Since Microsoft's compiler writes all warnings and errors to the standard output channel,
				// move them to the error channel

				while (outputLines.Count > 4)
				{
					var line = outputLines[3];
					outputLines.RemoveAt(3);
					errorLines.Add(line);
				}
			}

			Log("\n- Compiler output:");
			for (int i = 0; i < outputLines.Count; i++)
			{
				var line = outputLines[i];
				Console.Out.WriteLine(line);
				Log($"{i}: {line}");
			}

			Log("\n- Compiler errors:");
			for (int i = 0; i < errorLines.Count; i++)
			{
				var line = errorLines[i];
				Console.Error.WriteLine(line);
				Log($"{i}: {line}");
			}

			if (process.ExitCode != 0 || compilerVersion != CompilerVersion.Version6Microsoft)
			{
				return process.ExitCode;
			}

			Log("\n- PDB to MDB conversion --------------------------------------\n");

			var pdb2mdbPath = Path.Combine(Directory.GetCurrentDirectory(), @"Roslyn/pdb2mdb.exe");
			var libraryPath = Directory.GetFiles("Temp", "*.dll").First();
			var pdbPath = Path.Combine("Temp", Path.GetFileNameWithoutExtension(Directory.GetFiles("Temp", "*.dll").First()) + ".pdb");

			process = new Process();
			process.StartInfo.FileName = pdb2mdbPath;
			process.StartInfo.Arguments = libraryPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;

			Log($"Process: {process.StartInfo.FileName}");
			Log($"Arguments: {process.StartInfo.Arguments}");

			process.Start();
			process.WaitForExit();
			File.Delete(pdbPath);

			Log("\n- pdb2mdb.exe output:");
			Log(process.StandardOutput.ReadToEnd());
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.Write($"Compiler redirection error: {e.Message}\n{e.StackTrace}");
			return -1;
		}
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
				processArguments = $"-nostdlib+ -noconfig -r:\"{mscorlib}\" -r:\"{systemDllPath}\" -r:\"{systemCoreDllPath}\" -r:\"{systemXmlDllPath}\" {responseFile}";
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(version), version, null);
		}

		var process = new Process();
		process.StartInfo.FileName = processPath;
		process.StartInfo.Arguments = processArguments;
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