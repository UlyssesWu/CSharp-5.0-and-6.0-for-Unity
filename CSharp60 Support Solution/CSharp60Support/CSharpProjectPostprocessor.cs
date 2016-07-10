using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

// This script modifies .csproj files:
// 1) enables unsafe code,
// 2) removes C# 4.0 version restriction introduced by Visual Studio Tools for Unity.

[InitializeOnLoad]
public class CSharpProjectPostprocessor : AssetPostprocessor
{
	// In case VSTU is installed
	static CSharpProjectPostprocessor()
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.FullName.StartsWith("SyntaxTree.VisualStudio.Unity.Bridge") == false)
			{
				continue;
			}

			var projectFilesGeneratorType = assembly.GetType("SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator");
			if (projectFilesGeneratorType == null)
			{
				Debug.Log("Type 'SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator' not found");
				return;
			}

			var delegateType = assembly.GetType("SyntaxTree.VisualStudio.Unity.Bridge.FileGenerationHandler");
			if (delegateType == null)
			{
				Debug.Log("Type 'SyntaxTree.VisualStudio.Unity.Bridge.FileGenerationHandler' not found");
				return;
			}

			var projectFileGenerationField = projectFilesGeneratorType.GetField("ProjectFileGeneration", BindingFlags.Static | BindingFlags.Public);
			if (projectFileGenerationField == null)
			{
				Debug.Log("Field 'ProjectFileGeneration' not found");
				return;
			}

			var handlerMethodInfo = typeof(CSharpProjectPostprocessor).GetMethod(nameof(ModifyProjectFile), BindingFlags.Static | BindingFlags.NonPublic);
			var handlerDelegate = Delegate.CreateDelegate(delegateType, null, handlerMethodInfo);

			var delegateValue = (Delegate)projectFileGenerationField.GetValue(null);
			delegateValue = delegateValue == null ? handlerDelegate : Delegate.Combine(delegateValue, handlerDelegate);
			projectFileGenerationField.SetValue(null, delegateValue);

			return;
		}
	}

	// In case VSTU is not installed
	private static void OnGeneratedCSProjectFiles()
	{
		if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("SyntaxTree.VisualStudio.Unity.Bridge")))
		{
			return;
		}

		foreach (string projectFile in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj"))
		{
			string content = File.ReadAllText(projectFile);
			content = ModifyProjectFile(null, content);
			File.WriteAllText(projectFile, content);
		}
	}

	private static string ModifyProjectFile(string name, string content)
	{
		var xdoc = XDocument.Parse(content);

		RemoveLangVersionRestriction(xdoc);
		EnableUnsafeCode(xdoc);
		//RemoveAnnoyingReferences(xdoc);

		var writer = new Utf8StringWriter();
		xdoc.Save(writer);
		return writer.ToString();
	}

	private class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}

	private static void RemoveLangVersionRestriction(XDocument xdoc)
	{
		XNamespace ns = xdoc.Root.GetDefaultNamespace();

		xdoc.Descendants(ns + "LangVersion").Remove();
	}

	private static void EnableUnsafeCode(XDocument xdoc)
	{
		XNamespace ns = xdoc.Root.GetDefaultNamespace();

		var propertyGroups = xdoc.Descendants(ns + "PropertyGroup").Where(t => t.Attribute("Condition") != null);
		foreach (var propertyGroup in propertyGroups)
		{
			var element = propertyGroup.Element(ns + "AllowUnsafeBlocks");
			if (element != null)
			{
				element.Value = "True";
			}
			else
			{
				propertyGroup.Add(new XElement(ns + "AllowUnsafeBlocks", "True"));
			}
		}
	}

	private static void RemoveAnnoyingReferences(XDocument xdoc)
	{
		XNamespace ns = xdoc.Root.GetDefaultNamespace();

		(from element in xdoc.Descendants(ns + "Reference")
		 let include = element.Attribute("Include").Value
		 where include == "Boo.Lang" || include == "UnityScript.Lang"
		 select element).Remove();
	}

}