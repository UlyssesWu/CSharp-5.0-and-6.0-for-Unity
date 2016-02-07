# Can I use C# 6.0 in Unity? #

Yes, you can.

Unity has been stuck with CLR 2.0 for a very long time, but almost all the latest C# features do not require the latest versions of CLR. Microsoft and Mono compilers can compile C# 6.0 code for CLR 2.0 if you explicitly ask them to do so.

Late binding (`dynamic`) feature that came with C# 4.0 still won't be available in Unity, because it relies on CLR 4.0 that we don't have yet.


# Ok, what should I do? #

1. If you run Unity 4 on Mac OS X, download and install [Mono][mono]. If you don't then don't.

2. Copy `CSharp60Support` folder from this repository to your Unity project. It should be located in the project's root, next to the `Assets` folder.

3. Import [CSharp60Support for Unity X.unitypackage](https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/downloads) into your project.

4. Select `Reimport All` or just restart the editor.

5. If you use Visual Studio Tools for Unity 2.2 or later, you might also need to delete the existing .csproj files and let Unity to recreate them from scratch. See 'Other known issues' section for the details.

[Watch a demo](How_to_install.gif)

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

All the source code is published under [WTFPL version 2](http://www.wtfpl.net/about/).


# What platforms are "supported"? #

This hack seems to work on the major platforms:

* Windows (editor and standalone)
* Mac OS X (editor and standalone)
* Android
* iOS

Since WebGL doesn't offer any multithreading support, AsyncBridge and Task Parallel Library are not available for this platform. Caller Info attributes are also not available, because their support comes with AsyncBridge library.

AsyncBridge/TPL stuff is also not compatible with Windows Store Application platform (and probably all the platforms that use .Net runtime instead of Mono runtime) due to API differences between the recent versions of .Net Framework and the ancient version of TPL (System.Threading.dll) that comes with AsyncBridge. Namely, you can't use async/await, Caller Info attributes and everything from System.Threading.dll (concurrent collections for example).

WebPlayer platform is not supported and most likely will never be since it is about to be deprecated.

# Other known issues #

* C# 5.0/6.0 is not compatible with Unity Cloud Build service for obvious reason.

* Visual Studio Tools for Unity starting from v2.2 restricts C# language version to 4.0 inside Visual Studio project files (*.csproj) and [doesn't let you open Project Properties window](http://forum.unity3d.com/threads/visual-studio-tools-for-unity-2-2.384014/#post-2498322) to revert this setting back to default. This restriction doesn't affect compilation in Unity but makes Visual Studio highlight half of your C# 6.0 code in red.

    Solution: After you've added C# 6.0 support to a project, delete the existing .csproj files if there are any and let Unity regenerate them. The new files won't have that language version restriction.

* Using Mono C# 6.0 compiler may cause occasional Unity crashes while debugging in Visual Studio - http://forum.unity3d.com/threads/c-6-0.314297/page-2#post-2225696

* IL2CPP doesn't support exception filters added in C# 6.0 (ExceptionFiltersTest.cs).

* If a MonoBehaviour is declared inside a namespace, the source file should not contain any C# 6.0-specific language constructions before the MonoBehaviour declaration. Otherwise, the editor won't recognize the script as a MonoBehaviour component.

    Bad example:

        using UnityEngine;
        using static System.Math; // C# 6.0 syntax!

        namespace Foo
        {
	       class Baz
	       {
		      object Qux1 => null; // C# 6.0 syntax!
		      object Qux2 { get; } = null; // C# 6.0 syntax!
	       }

        	class Bar : MonoBehaviour { } // "No MonoBehaviour scripts in the file, or their names do not match the file name."
        }
    Good example:        

        using UnityEngine;

        namespace Foo
        {
	       class Bar : MonoBehaviour { } // ok

	       class Baz
	       {
		      object Qux1 => null;
		      object Qux2 { get; } = null;
	       }
        }


* There's a bug in Mono C# 6.0 compiler, related to null-conditional operator support (NullConditionalTest.cs):

        var foo = new[] { 1, 2, 3 };
        var bar = foo?[0];
        Debug.Log((foo?[0]).HasValue); // error CS1061: Type `int' does not 
        // contain a definition for `HasValue' and no extension method
        // `HasValue' of type `int' could be found. Are you missing an
        // assembly reference?

    Mono compiler thinks that `foo?[0]` is `int` while it's actually `Nullable<int>`. However, `bar`'s type is deduced correctly - `Nullable<int>`. 


# Want to talk about it? #

http://forum.unity3d.com/threads/c-6-0.314297/#post-2108999

   
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