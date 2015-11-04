using UnityEditor;
using UnityEditor.Modules;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;

internal class CustomCSharpLanguage : CSharpLanguage
{
	public override ScriptCompilerBase CreateCompiler(MonoIsland island, bool buildingForEditor, BuildTarget targetPlatform, bool runUpdater)
	{
		// This method almost exactly copies CSharpLanguage.CreateCompiler(...)

		CSharpCompiler cSharpCompiler = GetCSharpCompiler(targetPlatform, buildingForEditor, island._output);
		if (cSharpCompiler != CSharpCompiler.Mono)
		{
			if (cSharpCompiler == CSharpCompiler.Microsoft)
			{
				return new MicrosoftCSharpCompiler(island, runUpdater);
			}
		}
		return new CustomCSharpCompiler(island, runUpdater); // MonoCSharpCompiler is replaced with CustomCSharpCompiler
	}
}