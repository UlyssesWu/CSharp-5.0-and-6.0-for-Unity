using System.Diagnostics;
using System.IO;
using System.Linq;

internal class Mono60Compiler : Compiler
{
	public Mono60Compiler(Logger logger, string directory)
		: base(logger, Path.Combine(directory, "mcs.exe"), null) { }

	public override string Name => "Mono C# 6.0";

	protected override Process CreateCompilerProcess(Platform platform, string unityEditorDataDir, string responseFile)
	{
        var systemDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.dll");
        var systemCoreDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Core.dll");
        var systemXmlDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/System.Xml.dll");
        var mscorlibDllPath = Path.Combine(unityEditorDataDir, @"Mono/lib/mono/2.0/mscorlib.dll");

        string processArguments = "-nostdlib+ -noconfig -nologo "
                                  + $"-r:\"{mscorlibDllPath}\" "
                                  + $"-r:\"{systemDllPath}\" "
                                  + $"-r:\"{systemCoreDllPath}\" "
                                  + $"-r:\"{systemXmlDllPath}\" " + responseFile;

  //      string processArguments;
  //      if (platform == Platform.Windows)
		//{
		//	processArguments = $"-sdk:2 -debug+ -langversion:Default -r:\"{systemCoreDllPath}\" {responseFile}";
		//}
		//else
		//{
		//	processArguments = $"-sdk:2 -debug+ -langversion:Default {responseFile}";
		//}

		FixTvosIosIssue(responseFile.TrimStart('@'));

		var process = new Process();
		process.StartInfo = CreateOSDependentStartInfo(platform, ProcessRuntime.CLR40, compilerPath, processArguments, unityEditorDataDir);
		return process;
	}

	private void FixTvosIosIssue(string responseFile)
	{
		var lines = File.ReadAllLines(responseFile).Select(line => line.Replace('\\', '/')).ToList();

		var definedTVOS = lines.Contains("-define:UNITY_TVOS");
		if (definedTVOS == false)
		{
			lines.RemoveAll(line => line.Contains("/PlaybackEngines/AppleTVSupport/UnityEditor.iOS.Extensions."));
		}

		var definedIOS = lines.Contains("-define:UNITY_IOS");
		if (definedIOS == false)
		{
			lines.RemoveAll(line => line.Contains("/PlaybackEngines/iOSSupport/UnityEditor.iOS.Extensions."));
		}

		File.WriteAllLines(responseFile, lines.ToArray());
	}

	public static bool IsAvailable(string directory) => File.Exists(Path.Combine(directory, "mcs.exe"));
}