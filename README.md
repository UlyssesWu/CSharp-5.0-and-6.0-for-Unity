# Can I use C# 6.0 in Unity? #

Yes, you can.

Unity has been stuck with CLR 2.0 for a very long time, but almost all the latest C# features do not require the latest versions of CLR. Microsoft and Mono compilers can compile C# 6.0 code for CLR 2.0 if you explicitly ask them to do so.

Late binding (`dynamic`) feature that came with C# 4.0 still won't be available in Unity, because it relies on CLR 4.0 that we don't have yet.

# Ok, what should I do ? #

1. Copy `smcs\smcs\bin\Release\smcs.exe` from this repository to your `\Unity\Editor\Data\Mono\lib\mono\2.0` folder. Just copy, there's nothing to replace.

2. Create a new Unity project or open an existing one. Make sure that `Project Settings`/`Player`/`API Compatibility Level` is set to `.Net 2.0`.

3. Copy `mcs.exe` or `Roslyn` folder to your project's root.

That's all.

# How does it work? #

smcs.exe receives and redirects compilation requests from Unity to one of the available C# compilers using the following rules:

1. If the current project contains `Roslyn` folder, then Roslyn C# 6.0 compiler will be used;

2. else if the current project contains `mcs.exe`, then this Mono's 4.0.0 C# 6.0 compiler will be used;

3. else if there's `AsyncBridge.Net35.dll` somewhere inside the project, then Unity's C# 5.0 compiler will be used (\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe);

4. else the stock compiler will be used (\Unity\Editor\Data\Mono\lib\mono\2.0\gmcs.exe).

# License #

The source code is released under [WTFPL version 2](http://www.wtfpl.net/about/).

# Want to talk about it? #

http://forum.unity3d.com/threads/c-6-0.314297/#post-2108999

# Random notes #

* I have no idea what problems this hack may introduce. I hope none, but it needs to be tested.

* Roslyn compiler was built from the sources available on its [official repository on GitHub][roslyn]. No changes made.

* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.0.0][mono] installation. pdb2mdb.exe that comes with Unity is not compatible with the assemblies generated with Roslyn compiler.

* AsyncBridge library provides a set of types that makes it possible to use _async/await_ in projects that target CLR 2.0. For more information, check [this blog post][asyncbridge].

* If you use _async/await_ inside Unity events (Awake, Start, Update etc) you may notice that continuations (the code below `await` keyword) are executed in background threads. Most likely, this is not what you would want. To force `await` to return the execution to the main thread, you'll have to provide it with a synchronization context. Check `UnityScheduler.cs` example located inside the project.

    For more information about what synchronization context is, what it is for and how to use it, see this set of articles by Stephen Toub: [1][synccontext1], [2][synccontext2], [3][synccontext3].

* It looks like the Mono's 4.0.0 compiler doesn't fully understand null-conditional operators:

        var foo = new[] { 1, 2, 3 };
        var bar = foo?[0];

    It thinks that `bar`'s type is `int` while it should be `int?` or `Nullable<int>`.
[mono]: http://www.mono-project.com/download/#download-win
[roslyn]: https://github.com/dotnet/roslyn
[asyncbridge]: https://www.simple-talk.com/blogs/2012/04/18/asyncbridge-write-async-code-for-net-3-5/
[synccontext1]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
[synccontext2]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx
[synccontext3]: http://blogs.msdn.com/b/pfxteam/archive/2012/02/02/await-synchronizationcontext-and-console-apps-part-3.aspx