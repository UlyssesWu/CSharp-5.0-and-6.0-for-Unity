using System.IO;
using System.Xml.Linq;
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

		return true;
	}

	private static void RemoveLanguageVersionRestriction(string projectFile)
	{
		var xDocument = XDocument.Load(projectFile);
		var ns = "{http://schemas.microsoft.com/developer/msbuild/2003}";

		foreach (var propertyGroup in xDocument.Root.Elements(ns + "PropertyGroup"))
		{
			var langVersion = propertyGroup.Element(ns + "LangVersion");
			if (langVersion != null)
			{
				langVersion.Remove();
			}

			var defines = propertyGroup.Element(ns + "DefineConstants");
			if (defines != null)
			{
				if (defines.Value.Contains("__DEMO__;__DEMO_EXPERIMENTAL__") == false)
				{
					defines.Value += ";__DEMO__;__DEMO_EXPERIMENTAL__";
				}
			}
			else
			{
				var element = new XElement(ns + "DefineConstants");
				element.Value = "__DEMO__";
				propertyGroup.Add(element);
			}
		}

		xDocument.Save(projectFile);
	}
}