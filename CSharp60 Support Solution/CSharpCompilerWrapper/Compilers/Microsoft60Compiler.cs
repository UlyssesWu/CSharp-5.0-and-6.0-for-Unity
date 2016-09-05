﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

internal class Microsoft60Compiler : Compiler
{
	public override string Name => "Microsoft C# 6.0";
	public override bool NeedsPdb2MdbConversion => true;

	public Microsoft60Compiler(Logger logger, string directory)
		: base(logger, Path.Combine(directory, "csc.exe"), Path.Combine(directory, "pdb2mdb.exe")) { }

	public static bool IsAvailable(string directory) => File.Exists(Path.Combine(directory, "csc.exe")) &&
														File.Exists(Path.Combine(directory, "pdb2mdb.exe"));

	protected override Process CreateCompilerProcess(Platform platform, string unityEditorDataDir, string responseFile)
	{
		string systemDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.dll");
		string systemCoreDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Core.dll");
		string systemXmlDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Xml.dll");
		string mscorlibDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/mscorlib.dll");

		string processArguments = "-nostdlib+ -noconfig -nologo "
								  + $"-r:\"{mscorlibDllPath}\" "
								  + $"-r:\"{systemDllPath}\" "
								  + $"-r:\"{systemCoreDllPath}\" "
								  + $"-r:\"{systemXmlDllPath}\" " + responseFile;

		var process = new Process();
		process.StartInfo = CreateOSDependentStartInfo(platform, ProcessRuntime.CLR40, compilerPath, processArguments, unityEditorDataDir);
		return process;
	}

	public override void ConvertDebugSymbols(Platform platform, string targetAssemblyPath, string unityEditorDataDir)
	{
		outputLines.Clear();

		var process = new Process();
		process.StartInfo = CreateOSDependentStartInfo(platform, ProcessRuntime.CLR40, pbd2MdbPath, targetAssemblyPath, unityEditorDataDir);
		process.OutputDataReceived += (sender, e) => outputLines.Add(e.Data);

		logger?.Append($"Process: {process.StartInfo.FileName}");
		logger?.Append($"Arguments: {process.StartInfo.Arguments}");

		process.Start();
		process.BeginOutputReadLine();
		process.WaitForExit();
		logger?.Append($"Exit code: {process.ExitCode}");

		string pdbPath = Path.Combine("Temp", Path.GetFileNameWithoutExtension(targetAssemblyPath) + ".pdb");
		File.Delete(pdbPath);
	}

	public override void PrintCompilerOutputAndErrors()
	{
		// Microsoft's compiler writes all warnings and errors to the standard output channel,
		// so move them to the error channel

		errorLines.AddRange(outputLines);
		outputLines.Clear();

		base.PrintCompilerOutputAndErrors();
	}

	public override void PrintPdb2MdbOutputAndErrors()
	{
		var lines = (from line in outputLines
					 let trimmedLine = line?.Trim()
					 where string.IsNullOrEmpty(trimmedLine) == false
					 select trimmedLine).ToList();

		logger?.Append($"- pdb2mdb.exe output ({lines.Count} {(lines.Count == 1 ? "line" : "lines")}):");

		for (int i = 0; i < lines.Count; i++)
		{
			Console.Out.WriteLine(lines[i]);
			logger?.Append($"{i}: {lines[i]}");
		}
	}
}