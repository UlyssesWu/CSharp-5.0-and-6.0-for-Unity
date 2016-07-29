﻿#define LOGGING_ENABLED

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

internal class Program
{
	private const string LANGUAGE_SUPPORT_DIR = "CSharp60Support";

	private static int Main(string[] args)
	{
		int exitCode;
		Logger logger = null;

#if LOGGING_ENABLED
		using (logger = new Logger())
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

		var responseFile = args[0];
		var compilationOptions = File.ReadAllLines(responseFile.TrimStart('@'));
		var unityEditorDataDir = GetUnityEditorDataDir();
		var projectDir = Directory.GetCurrentDirectory();
		var targetAssembly = compilationOptions.First(line => line.StartsWith("-out:")).Substring(10).Trim('\'');

		logger?.Append($"CSharpCompilerWrapper.exe version: {Assembly.GetExecutingAssembly().GetName().Version}");
		logger?.Append($"Platform: {CurrentPlatform}");
		logger?.Append($"Target assembly: {targetAssembly}");
		logger?.Append($"Project directory: {projectDir}");
		logger?.Append($"Unity 'Data' or 'Frameworks' directory: {unityEditorDataDir}");

		if (CurrentPlatform == Platform.Linux)
		{
			logger?.Append("");
			logger?.Append("Platform is not supported");
			return -1;
		}

		var compiler = FindSuitableCompiler(logger, CurrentPlatform, projectDir, compilationOptions, unityEditorDataDir);

		logger?.Append($"Compiler: {compiler.Name}");
		logger?.Append("");
		logger?.Append("- Compilation -----------------------------------------------");
		logger?.Append("");

		var stopwatch = Stopwatch.StartNew();
		var exitCode = compiler.Compile(CurrentPlatform, unityEditorDataDir, responseFile);
		stopwatch.Stop();

		logger?.Append($"Elapsed time: {stopwatch.ElapsedMilliseconds / 1000f:F2} sec");
		logger?.Append("");
		compiler.PrintCompilerOutputAndErrors();

		if (exitCode != 0 || compiler.NeedsPdb2MdbConversion == false)
		{
			return exitCode;
		}

		logger?.Append("");
		logger?.Append("- PDB to MDB conversion --------------------------------------");
		logger?.Append("");

		stopwatch.Reset();
		stopwatch.Start();

		var targetAssemblyPath = Path.Combine("Temp", targetAssembly);
		compiler.ConvertDebugSymbols(CurrentPlatform, targetAssemblyPath, unityEditorDataDir);

		stopwatch.Stop();
		logger?.Append($"Elapsed time: {stopwatch.ElapsedMilliseconds / 1000f:F2} sec");
		logger?.Append("");
		compiler.PrintPdb2MdbOutputAndErrors();

		return 0;
	}

	private static Compiler FindSuitableCompiler(Logger logger, Platform platform, string projectDir, string[] compilationOptions, string unityEditorDataDir)
	{
		Compiler compiler = null;

		// Looking for Roslyn C# 6.0 compiler
		var roslynDirectory = Path.Combine(Path.Combine(projectDir, LANGUAGE_SUPPORT_DIR), "Roslyn");
		if (Microsoft60Compiler.IsAvailable(roslynDirectory))
		{
			compiler = new Microsoft60Compiler(logger, roslynDirectory);
		}

		if (compiler != null && platform != Platform.Windows)
		{
			compiler = null;
			logger?.Append("Microsoft C# 6.0 compiler found, but it is not supported on the current platform. Looking for another compiler...");
		}

		if (compiler == null)
		{
			// Looking for Mono C# 6.0 compiler
			var mcsDirectory = Path.Combine(projectDir, LANGUAGE_SUPPORT_DIR);
			if (Mono60Compiler.IsAvailable(mcsDirectory))
			{
				compiler = new Mono60Compiler(logger, mcsDirectory);
			}
		}

		if (compiler == null)
		{
			// Using stock Mono C# 3.0 compiler
			var stockCompilerPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/gmcs.exe");
			compiler = new Mono30Compiler(logger, stockCompilerPath);
		}

		return compiler;
	}

	private static Platform CurrentPlatform
	{
		get
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Unix:
					// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
					// Instead of platform check, we'll do a feature checks (Mac specific root folders)
					if (Directory.Exists("/Applications")
						& Directory.Exists("/System")
						& Directory.Exists("/Users")
						& Directory.Exists("/Volumes"))
					{
						return Platform.Mac;
					}
					return Platform.Linux;

				case PlatformID.MacOSX:
					return Platform.Mac;

				default:
					return Platform.Windows;
			}
		}
	}

	/// <summary>
	/// Returns the directory that contains Mono and MonoBleedingEdge directories
	/// </summary>
	private static string GetUnityEditorDataDir()
	{
		// Windows:
		// MONO_PATH: C:\Program Files\Unity\Editor\Data\Mono\lib\mono\2.0
		//
		// Mac OS X:
		// MONO_PATH: /Applications/Unity/Unity.app/Contents/Frameworks/Mono/lib/mono/2.0

		var monoPath = Environment.GetEnvironmentVariable("MONO_PATH").Replace("\\", "/");
		var index = monoPath.IndexOf("/Mono/lib/", StringComparison.InvariantCultureIgnoreCase);
		var path = monoPath.Substring(0, index);
		return path;
	}
}