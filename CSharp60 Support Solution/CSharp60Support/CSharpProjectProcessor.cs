using System.IO;
using System.Linq;
using UnityEditor;

public class CSharpProjectProcessor : AssetPostprocessor
{
	private static bool OnPreGeneratingCSProjectFiles()
	{
		var currentDirectory = Directory.GetCurrentDirectory();
		var projectFiles = Directory.GetFiles(currentDirectory, "*.csproj");

		foreach (var file in projectFiles)
		{
			RemoveLanguageVersionRestriction(file);
		}

		return false;
	}

	private static void RemoveLanguageVersionRestriction(string projectFile)
	{
		var lines = File.ReadAllLines(projectFile);
		lines = lines.Where(line => line.Contains("LangVersion") == false).ToArray();
		File.WriteAllLines(projectFile, lines);
	}
}