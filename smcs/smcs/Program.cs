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

	private static readonly List<string> OutputLines = new List<string>();
	private static readonly List<string> ErrorLines = new List<string>();

	private static int Main(string[] args)
	{
		int exitCode;
		Logger logger = null;

#if LOGGING_ENABLED
		logger = new Logger();
		using (logger)
#endif
		{
			try
			{
				exitCode = Compile(args, logger);
			}
			catch (Exception e)
			{
				exitCode = -1;
				Console.Error.Write($"Compiler redirection error: {e.GetType()}{Environment.NewLine}{e.Message} {e.StackTrace}");
			}
		}

		return exitCode;
	}

	private static int Compile(string[] args, Logger logger)
	{
		logger?.AppendHeader();

		var compilationOptions = GetCompilationOptions(args);
		var unityEditorDataDir = GetUnityEditorDataDir(compilationOptions);
		var targetAssembly = compilationOptions.First(line => line.StartsWith("-out:")).Substring(10);

		logger?.Append($"smcs.exe version: {Assembly.GetExecutingAssembly().GetName().Version}");
		logger?.Append($"Target assembly: {targetAssembly}");
		logger?.Append($"Project directory: {Directory.GetCurrentDirectory()}");
		logger?.Append($"Unity directory: {unityEditorDataDir}");

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

		logger?.Append($"Compiler: {compilerVersion}");
		logger?.Append("");
		logger?.Append("- Compilation -----------------------------------------------");
		logger?.Append("");

		var stopwatch = Stopwatch.StartNew();
		var process = CreateCompilerProcess(compilerVersion, unityEditorDataDir, args[0]);

		logger?.Append($"Process: {process.StartInfo.FileName}");
		logger?.Append($"Arguments: {process.StartInfo.Arguments}");

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();

		stopwatch.Stop();
		logger?.Append($"Exit code: {process.ExitCode}");
		logger?.Append($"Elapsed time: {stopwatch.ElapsedMilliseconds / 1000f:F2} sec");

		if (compilerVersion == CompilerVersion.Version6Microsoft)
		{
			// Microsoft's compiler writes all warnings and errors to the standard output channel,
			// so move them to the error channel

			while (OutputLines.Count > 4)
			{
				var line = OutputLines[3];
				OutputLines.RemoveAt(3);
				ErrorLines.Add(line);
			}
		}

		logger?.Append("");
		logger?.Append("- Compiler output:");

		var lines = from line in OutputLines
					let trimmedLine = line?.Trim()
					where string.IsNullOrEmpty(trimmedLine) == false
					select trimmedLine;

		int lineIndex = 0;
		foreach (var line in lines)
		{
			Console.Out.WriteLine(line);
			logger?.Append($"{lineIndex++}: {line}");
		}

		logger?.Append("");
		logger?.Append("- Compiler errors:");

		lines = from line in ErrorLines
				let trimmedLine = line?.Trim()
				where string.IsNullOrEmpty(trimmedLine) == false
				select trimmedLine;

		lineIndex = 0;
		foreach (var line in lines)
		{
			Console.Error.WriteLine(line);
			logger?.Append($"{lineIndex++}: {line}");
		}

		logger?.Append("");

		if (process.ExitCode != 0 || compilerVersion != CompilerVersion.Version6Microsoft)
		{
			return process.ExitCode;
		}

		logger?.Append("- PDB to MDB conversion --------------------------------------");
		logger?.Append("");

		OutputLines.Clear();
		ErrorLines.Clear();

		var pdb2mdbPath = Path.Combine(Directory.GetCurrentDirectory(), @"Roslyn/pdb2mdb.exe");
		var libraryPath = Path.Combine("Temp", targetAssembly);
		var pdbPath = Path.Combine("Temp", Path.GetFileNameWithoutExtension(targetAssembly) + ".pdb");

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

		logger?.Append($"Process: {process.StartInfo.FileName}");
		logger?.Append($"Arguments: {process.StartInfo.Arguments}");

		stopwatch.Reset();
		stopwatch.Start();

		process.Start();
		process.BeginOutputReadLine();
		process.WaitForExit();

		stopwatch.Stop();
		logger?.Append($"Elapsed time: {stopwatch.ElapsedMilliseconds / 1000f:F2} sec");

		File.Delete(pdbPath);

		logger?.Append("");
		logger?.Append("- pdb2mdb.exe output:");
		foreach (var line in OutputLines)
		{
			logger?.Append(line);
		}

		return 0;
	}

	private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		OutputLines.Add(e.Data);
	}

	private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		ErrorLines.Add(e.Data);
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