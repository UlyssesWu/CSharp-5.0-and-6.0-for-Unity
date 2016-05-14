using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CSharpProjectPostprocessor
{
	static CSharpProjectPostprocessor()
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.FullName.StartsWith("SyntaxTree.VisualStudio.Unity.Bridge") == false)
			{
				continue;
			}

			var projectFilesGeneratorType = assembly.GetType("SyntaxTree.VisualStudio.Unity.Bridge.ProjectFilesGenerator");
			if (projectFilesGeneratorType != null)
			{
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

				var methodInfo = typeof(CSharpProjectPostprocessor).GetMethod(nameof(RemoveLanguageVersionRestriction), BindingFlags.Static | BindingFlags.NonPublic);
				var postprocessHandler = Delegate.CreateDelegate(delegateType, null, methodInfo);

				var delegateValue = (Delegate)projectFileGenerationField.GetValue(null);
				delegateValue = delegateValue == null ? postprocessHandler : Delegate.Combine(delegateValue, postprocessHandler);
				projectFileGenerationField.SetValue(null, delegateValue);

				return;
			}
		}
	}

	private static string RemoveLanguageVersionRestriction(string name, string content)
	{
		return content.Replace("<LangVersion Condition=\" '$(VisualStudioVersion)' != '10.0' \">4</LangVersion>", string.Empty);
	}
}