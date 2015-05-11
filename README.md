# How to use? #

1. Copy `smcs\smcs\bin\Release\smcs.exe` from this repositoty to your `\Unity\Editor\Data\Mono\lib\mono\2.0` folder. Just copy, there's nothing to replace.

2. Create a new or open an old Unity project. Switch `Project Settings`/`Player`/`API Compatibility Level` option to `.Net 2.0`.

3. Copy `mcs.exe` or `Roslyn` folder to your project's root folder.

That's all.

# How does it work? #

smcs.exe receives and redirects compilation requests from Unity to one of the available C# compilers using the following rules:

1. If the current project contains `Roslyn` folder, then this Roslyn C# 6.0 compiler will be called;

2. else if the current project contains `mcs.exe`, then this Mono's 4.0.0 C# 6.0 compiler will be called;

3. else if there's `AsyncBridge.Net35.dll` somewhere inside the project, then Unity's C# 5.0 compiler will be called (\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe);

4. else the stock compiler will be called (\Unity\Editor\Data\Mono\lib\mono\2.0\gmcs.exe).

# License #

The code is released under [WTFPL version 2](http://www.wtfpl.net/about/).

# Random notes #

* I have no idea what problems this hack may introduce. I hope none, but it needs to be tested.


* Roslyn compiler was built from the sources available on its [official repository on GitHub][roslyn]. No changes made.


* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.0.0][mono] installation.


* AsyncBridge library provides a set of types that makes it possible to use async/await in projects that target CLR2.0. For more information, check [this blog post][asyncbridge].


* If you use _async/await_ inside Unity events (Awake, Start, Update etc) you may notice that continuations (the code below `await` keyword) are executed in background threads. It's most likely that this is not what you want. To force `await` to return the execution to the main thread, you'll have to provide it with a synchronization context. Check `UnityScheduler.cs` example provided with this project.

    For more information about what synchronization context is, what it is for and how to use it, see this set of articles by Stephen Toub: [1][synccontext1], [2][synccontext2], [3][synccontext3].

[mono]: http://www.mono-project.com/download/#download-win
[roslyn]: https://github.com/dotnet/roslyn
[asyncbridge]: https://www.simple-talk.com/blogs/2012/04/18/asyncbridge-write-async-code-for-net-3-5/
[synccontext1]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
[synccontext2]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx
[synccontext3]: http://blogs.msdn.com/b/pfxteam/archive/2012/02/02/await-synchronizationcontext-and-console-apps-part-3.aspx