# Can I use C# 6.0 in Unity? #

Yes, you can.

Unity has been stuck with CLR 2.0 for a very long time, but almost all the latest C# features do not require the latest versions of CLR. Microsoft and Mono compilers can compile C# 6.0 code for CLR 2.0 if you explicitly ask them to do so.

Late binding (`dynamic`) feature that came with C# 4.0 still won't be available in Unity, because it relies on CLR 4.0 that we don't have yet.

# Ok, what should I do? #

1. If you run Unity 4 on Mac OS X, download and install [Mono][mono]. If you don't then don't.

2. Copy `CSharp60Support` folder from this repository to your Unity project. It should be located in the project's root, next to the `Assets` folder.

3. If you use Unity 5, import `CSharp60Support/CSharp60Support for Unity 5.unitypackage` into your project.

    If you use Unity 4, import `CSharp60Support/CSharp60Support for Unity 4.unitypackage` into your project.

4. Select `Reimport All` or just restart the editor.

[Watch animated gif.](How_to_install.gif)

Thus, the project folder is the only folder that changes. All the other projects will work as usual.

# How does it work? #

1. `UnityProject/Assets/CSharp 6.0 Support/Editor/CSharp60Support.dll` is an editor extension that modifies the editor's internal data via reflection, telling it to use the alternative C# compiler (``UnityProject/CSharp60Support/CSharpCompilerWrapper.exe``). If it doesn't exist, the stock compiler will be used.

2. `CSharpCompilerWrapper.exe` receives and redirects compilation requests from Unity to one of the actual C# compilers using the following rules:

    * If `CSharp60Support` folder contains `Roslyn` folder and the platform is Windows, then Roslyn C# 6.0 compiler will be used;

    * else if `CSharp60Support` folder contains `mcs.exe`, then this Mono C# 6.0 compiler will be used;

    * else if there's `AsyncBridge.Net35.dll` somewhere inside the project, then Unity's C# 5.0 compiler will be used (`/Unity/Editor/Data/MonoBleedingEdge/lib/mono/4.5/mcs.exe`);

    * else the stock compiler will be used (`/Unity/Editor/Data/Mono/lib/mono/2.0/gmcs.exe`).
    
To make sure that `CSharpCompilerWrapper.exe` does actually work, check its log file: `UnityProject/CSharp60Support/compilation log.txt`

# License #

The source code is released under [WTFPL version 2](http://www.wtfpl.net/about/).

# Want to talk about it? #

http://forum.unity3d.com/threads/c-6-0.314297/#post-2108999

# Known issues #

* C# 5.0/6.0 is not compatible with Unity Cloud Build service for obvious reason.

* WebPlayer platform is not supported. No serious problems, it just requires some additional effort.

* AsyncBrigde stuff is not compatible with Windows Store Application platform due to API differences between the recent versions of .Net Framework and the ancient version of System.Threading.dll that comes with AsyncBridge. Namely, you can't use async/await, caller information attributes and everything from System.Threading.dll (concurrent collections for example).

* Using Mono C# 6.0 compiler may cause Unity crashes while debugging in Visual Studio - http://forum.unity3d.com/threads/c-6-0.314297/page-2#post-2225696

* IL2CPP doesn't support exception filters added in C# 6.0 (ExceptionFiltersTest.cs).

* There are cases when Mono compiler fails to compile fully legit C# 6.0 code:

    * Null-conditional operator *(NullConditionalTest.cs)*

            var foo = new[] { 1, 2, 3 };
            var bar = foo?[0];
            Debug.Log((foo?[0]).HasValue); // error CS1061: Type `int' does not 
            // contain a definition for `HasValue' and no extension method
            // `HasValue' of type `int' could be found. Are you missing an
            // assembly reference?

        Mono compiler thinks that `foo?[0]` is `int` while it's actually `Nullable<int>`. However, `bar`'s type is deduced correctly - `Nullable<int>`. 
    
# Random notes #

* Roslyn compiler was taken from VS 2015 installation.

* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.2.1.102][mono] installation. pdb2mdb.exe that comes with Unity is not compatible with the assemblies generated with Roslyn compiler.

* AsyncBridge library contains a set of types that makes it possible to use _async/await_ in projects that target CLR 2.0. It also provides Caller Info attributes support. For more information, check [this blog post][asyncbridge].

* If you use _async/await_ inside Unity events (Awake, Start, Update etc) you may notice that continuations (the code below `await` keyword) are executed in background threads. Most likely, this is not what you would want. To force `await` to return the execution to the main thread, you'll have to provide it with a synchronization context, like all WinForms and WPF applications do.

    Check `UnityScheduler.cs` example implementation located inside the project or just put `UnityScheduler` prefab in your first scene. The script creates and registers a synchronization context for the Unity's main thread, so async/await could work the way they do in regular WinForms or WPF applications.

    For more information about what synchronization context is, what it is for and how to use it, see this set of articles by Stephen Toub: [one][synccontext1], [two][synccontext2], [three][synccontext3].

[mono]: http://www.mono-project.com/download/
[roslyn]: https://github.com/dotnet/roslyn
[asyncbridge]: https://www.simple-talk.com/blogs/2012/04/18/asyncbridge-write-async-code-for-net-3-5/
[synccontext1]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
[synccontext2]: http://blogs.msdn.com/b/pfxteam/archive/2012/01/21/10259307.aspx
[synccontext3]: http://blogs.msdn.com/b/pfxteam/archive/2012/02/02/await-synchronizationcontext-and-console-apps-part-3.aspx