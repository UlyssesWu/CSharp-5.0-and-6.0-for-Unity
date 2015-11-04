using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;
using UnityEditor.Utils;
using UnityEngine;

internal class CustomCSharpCompiler : MonoCSharpCompiler
{
	public CustomCSharpCompiler(MonoIsland island, bool runUpdater) : base(island, runUpdater)
	{
	}

	private string GetCompilerPath(List<string> arguments)
	{
		var compilerPath = Path.Combine(Directory.GetCurrentDirectory(), "smcs.exe");
		if (File.Exists(compilerPath))
		{
			return compilerPath;
		}

		Debug.LogWarning($"Custom C# compiler not found in project directory ({compilerPath}), using the default compiler");

		// calling base method via reflection
		var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
		var methodInfo = GetType().BaseType.GetMethod(nameof(GetCompilerPath), bindingFlags);
		var result = (string)methodInfo.Invoke(this, new object[] {arguments});
		return result;
	}

	private string[] GetAdditionalReferences()
	{
		// calling base method via reflection
		var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
		var methodInfo = GetType().BaseType.GetMethod(nameof(GetAdditionalReferences), bindingFlags);
		var result = (string[])methodInfo.Invoke(this, null);
		return result;
	}

	// Copy of MonoCSharpCompiler.StartCompiler()
	// The only reason it exists is to call the new implementation
	// of GetCompilerPath(...) which is non-virtual unfortunately.
	protected override Program StartCompiler()
	{
		var arguments = new List<string>
				   {
					   "-debug",
					   "-target:library",
					   "-nowarn:0169",
					   "-out:" + PrepareFileName(_island._output)
				   };
		foreach (var reference in _island._references)
		{
			arguments.Add("-r:" + PrepareFileName(reference));
		}

		foreach (var define in _island._defines.Distinct())
		{
			arguments.Add("-define:" + define);
		}

		foreach (var file in _island._files)
		{
			arguments.Add(PrepareFileName(file));
		}

		var additionalReferences = GetAdditionalReferences();
		foreach (string path in additionalReferences)
		{
			var text = Path.Combine(GetProfileDirectory(), path);
			if (File.Exists(text))
			{
				arguments.Add("-r:" + PrepareFileName(text));
			}
		}

		return StartCompiler(_island._target, GetCompilerPath(arguments), arguments);
	}
}