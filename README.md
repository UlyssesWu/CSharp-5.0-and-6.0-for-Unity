# Can I use C# 6.0 in Unity? #

Yes, you can.

Unity has been stuck with CLR 2.0 for a very long time, but almost all the latest C# features do not require the latest versions of CLR. Microsoft and Mono compilers can compile C# 6.0 code for CLR 2.0 if you explicitly ask them to do so.

Late binding (`dynamic`) feature that came with C# 4.0 still won't be available in Unity, because it relies on CLR 4.0 that we don't have yet.

# Ok, what should I do ? #

Old way:

1. Copy `smcs.exe` from this repository to your `/Unity/Editor/Data/Mono/lib/mono/2.0` folder on Windows or `/Applications/Unity/Unity.app/Contents/Frameworks/Mono/lib/mono/2.0` on Mac OS X. Just copy, there's nothing to replace.

2. Create a new Unity project or open an existing one. Make sure that `Project Settings`/`Player`/`API Compatibility Level` is set to `.Net 2.0`.

3. Copy `mcs.exe` or `Roslyn` folder to your project's root.

New way:

1. Copy `Assets/Editor/CSharp60Support.dll` from this repository to any `Editor` folder inside your project.

2. Copy `smcs.exe` to your project's root.

3. Copy `mcs.exe` or `Roslyn` folder to your project's root.

That's it.

The new way has one huge advantage: there's no need to modify Unity installation folder each time Unity updates.

# How does it work? #

`Assets/Editor/CSharp60Support.dll`, if exists, modifies Unity editor's internal data, telling it to use a custom C# compiler (`smcs.exe`) that should be located in the current project's root folder. If there's no `smcs.exe` in the project's root, then a stock compiler will be used.

`smcs.exe` receives and redirects compilation requests from Unity to one of the actual C# compilers using the following rules:

1. If the current project contains `Roslyn` folder and the platform is Windows, then Roslyn C# 6.0 compiler will be used;

2. else if the current project contains `mcs.exe`, then this Mono C# 6.0 compiler will be used;

3. else if there's `AsyncBridge.Net35.dll` somewhere inside the project, then Unity's C# 5.0 compiler will be used (/Unity/Editor/Data/MonoBleedingEdge/lib/mono/4.5/mcs.exe);

4. else the stock compiler will be used (/Unity/Editor/Data/Mono/lib/mono/2.0/gmcs.exe).

All this means that Unity will use the alternative compiler only in those projects, where you have explicitely expressed your wish to do so. Otherwise, it will use the stock compiler as usual.

# License #

The source code is released under [WTFPL version 2](http://www.wtfpl.net/about/).

# Want to talk about it? #

http://forum.unity3d.com/threads/c-6-0.314297/#post-2108999

# Known issues #

* Using Mono C# 6.0 compiler may cause Unity crashes while debugging in Visual Studio - http://forum.unity3d.com/threads/c-6-0.314297/page-2#post-2225696

* There are cases when Mono compiler fails to compile fully legit C# 6.0 code:

    * Null-conditional operator *(NullConditionalTest.cs)*

            var foo = new[] { 1, 2, 3 };
            var bar = foo?[0];
            Debug.Log((foo?[0]).HasValue); // error CS1061: Type `int' does not 
            // contain a definition for `HasValue' and no extension method
            // `HasValue' of type `int' could be found. Are you missing an
            // assembly reference?

        Mono compiler thinks that `foo?[0]` is `int` while it's actually `Nullable<int>`. However, `bar`'s type is deduced correctly - `Nullable<int>`. 
    
    * Getter-only auto-property initialization *(PropertyInitializerTest.cs)*
    
            class Abc { }

            class Test
            {
	           public Abc Abc { get; }
	           public Test()
	           {
		          Abc = new Abc(); // error CS0118: `Abc' is a `type' but a `variable' was expected
	           }
            }

* IL2CPP (affects iOS and WebGL):

    * Currently fails to process exception filters *(ExceptionFiltersTest.cs)*.

# Random notes #

* Roslyn compiler was taken from VS 2015 installation.

* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.0.4][mono] installation. pdb2mdb.exe that comes with Unity is not compatible with the assemblies generated with Roslyn compiler.

* AsyncBridge library provides a set of types that makes it possible to use _async/await_ in projects that target CLR 2.0. For more information, check [this blog post][asyncbridge].

* If you use _async/await_ inside Unity events (Awake, Start, Update etc) you may notice that continuations (the code below `await` keyword) are executed in background threads. Most likely, this is not what you would want. To force `await` to return the execution to the main thread, you'll have to provide it with a synchronization context. Check `UnityScheduler.cs` example located inside the project.

    For more information about what synchronization context is, what it is for and how to use it, see this set of articles by Stephen Toub: [one][synccontext1], [two][synccontext2], [three][synccontext3].

[mono]: http://www.mono-project.com/download/#download-win
[roslyn]: https://github.com/dotnet/roslyn
[asyncbridge]: https://www.simple-talk.com/blogs/2012/04/18/asyncbridge-write-async-code-for-net-3-5/
[synccontext1]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
[synccontext2]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx
[synccontext3]: http://blogs.msdn.com/b/pfxteam/archive/2012/02/02/await-synchronizationcontext-and-console-apps-part-3.aspx