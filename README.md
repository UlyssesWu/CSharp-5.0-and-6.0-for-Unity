# How to use? #

1. Copy `smcs\smcs\bin\Release\smcs.exe` from this repositoty to your `\Unity\Editor\Data\Mono\lib\mono\2.0` folder. Just copy, there's nothing to replace.

2. Create a new or open an old Unity project. Switch `Project Settings`/`Player`/`API Compatibility Level` option to `.Net 2.0`.

3. Copy `mcs.exe` or `Roslyn` folder to your project's root folder.

That's all.

# How does it work? #

smcs.exe receives and redirects compilations requests from Unity to one of the available C# compilers using the following rules:

1. If the current project contains `Roslyn` folder, then this Roslyn C# 6.0 compiler will be called;

2. else if the current project contains `mcs.exe`, then this Mono's 4.0.0 C# 6.0 compiler will be called;

3. else if there's `AsyncBridge.Net35.dll` somewhere inside the project, then Unity's C# 5.0 compiler will be called (\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe);

4. else the stock compiler will be called (\Unity\Editor\Data\Mono\lib\mono\2.0\gmcs.exe).

# Random notes #

* I have no idea what problems this hack may introduce. I hope none, but it needs to be tested.
* Roslyn compiler was built from the sources available at its [official repository on GitHub][roslyn]. No changes made.
* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.0.0][mono] installation.

[mono]: http://www.mono-project.com/download/#download-win
[roslyn]: https://github.com/dotnet/roslyn