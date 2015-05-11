#define LOG

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

			var process = CreateCompilerProcess(compilerVersion, unityEditorDataDir, args[0]);
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.Start();

			Log($"Process: {process.StartInfo.FileName}");
			Log($"Arguments: {process.StartInfo.Arguments}");

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
			process.WaitForExit();
			Log($"Exit code: {process.ExitCode}");

			if (compilerVersion == CompilerVersion.Version6Microsoft)
			{
				output = output.Replace("\r\n", "\n");
				error = error.Replace("\r\n", "\n");

				if (process.ExitCode != 0)
				{
					Console.Error.Write(output);
					Console.Error.Write(error);
				}
				else
				{
					Console.Error.Write(error);
					Console.Out.Write(output);
				}
			}
			else
			{
				Console.Error.Write(error);
				Console.Out.Write(output);
			}

#if LOG
			Log("\n- Compiler output: ------");
			var lines = output.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				Log($"output{i}: {lines[i]}");
			}
			Log("\n- Compiler errors: ------");
			lines = error.Split('\n', '\r');
			for (int i = 0; i < lines.Length; i++)
			{
				Log($"error{i}: {lines[i]}");
			}
#endif

			if (process.ExitCode != 0 || compilerVersion != CompilerVersion.Version6Microsoft)
			{
				return process.ExitCode;
			}

			Log("\n- Symbol DB convertion: -");

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

			Log("\n- pdb2mdb.exe output: ---");
			Log(process.StandardOutput.ReadToEnd());
			return 0;
		}
		catch (Exception e)
		{
			Console.Error.Write($"Compiler redirection error: {e.Message}\n{e.StackTrace}");
			return -1;
		}
	}

	private const string LOG_FILENAME = "log.txt";

	[Conditional("LOG")]
	private static void InitLog()
	{
		File.WriteAllText(LOG_FILENAME, "");
	}

	[Conditional("LOG")]
	private static void Log(string message)
	{
		File.AppendAllText(LOG_FILENAME, message + "\n");
	}

	private static Process CreateCompilerProcess(CompilerVersion version, string unityEditorDataDir, string responseFile)
	{
		string processPath;
		string processArguments;

		var systemCoreDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Core.dll");

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
				processArguments = $"-nostdlib+ -noconfig -r:\"{mscorlib}\" -r:\"{systemCoreDllPath}\" {responseFile}";
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
		var dir = Directory.GetParent(filename).Parent;
		return dir.FullName;
	}

	private static string[] GetCompilationOptions(string[] args)
	{
		var compilationOptions = File.ReadAllLines(args[0].TrimStart('@'));
		return compilationOptions;
	}
}