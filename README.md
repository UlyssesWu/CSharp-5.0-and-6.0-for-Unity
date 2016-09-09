# Can I use C# 6.0 in Unity? #

Yes, you can.

Unity has been stuck with CLR 2.0 for a very long time, but almost all the latest C# features do not require the latest versions of CLR. Microsoft and Mono compilers can compile C# 6.0 code for CLR 2.0 if you explicitly ask them to do so.

Late binding (`dynamic`) feature that came with C# 4.0 still won't be available in Unity.


# Ok, what should I do? #

1. If you run Unity 4 on Mac OS X, download and install [Mono][mono]. If you don't then don't.

2. Copy `CSharp60Support` folder from this repository (or the [downloads page][downloads]) to your Unity project. It should be placed in the project's root, next to the `Assets` folder.

3. Import `CSharp60Support for Unity X.unitypackage` into your project. It's located inside `CSharp60Support` folder.

4. Select `Reimport All` or just restart the editor, whatever is faster in your case.

5. [Optional] If you use Windows, run `/CSharp60Support/ngen install.cmd` once *with administrator privileges*. It will precompile csc.exe, pdb2mdb.exe and mcs.exe using [Ngen][ngen] that will make compilation in Unity a bit faster.

[Watch a demo](How_to_install.gif)

Thus, the project folder is the only folder that changes. All the other projects will work as usual.


# How does it work? #

1. `/Assets/CSharp 6.0 Support/Editor/CSharp60Support.dll` is an editor extension that modifies the editor's internal data via reflection, telling it to use the alternative C# compiler (`/CSharp60Support/CSharpCompilerWrapper.exe`). If it doesn't exist, the stock compiler will be used.

2. `CSharpCompilerWrapper.exe` receives and redirects compilation requests from Unity to one of the actual C# compilers using the following rules:

    * If `CSharp60Support` folder contains `Roslyn` folder and the platform is Windows, then Roslyn C# 6.0 compiler will be used;

    * else if `CSharp60Support` folder contains `mcs.exe`, then this Mono C# 6.0 compiler will be used;

    * else the stock compiler will be used (`/Unity/Editor/Data/Mono/lib/mono/2.0/gmcs.exe`).
    
To make sure that `CSharpCompilerWrapper.exe` does actually work, check its log file: `UnityProject/CSharp60Support/compilation.log`


# Response (.rsp) files #

If you want to use a response file to pass extra options to the compiler (e.g. `-unsafe`), the file must be named `CSharpCompilerWrapper.rsp`.


# Versions of Roslyn compiler #

Roslyn compiler in C# 7.0 preview packages available on the [downloads page][downloads] is taken from the latest Visual Studio 15 Preview version (currently Preview 4).

Regular packages contain Roslyn from Visual Studio 2015.


# What platforms are "supported"? #

This hack seems to work on the major platforms:

* Windows (editor and standalone)
* Mac OS X (editor and standalone)
* Android
* iOS

Roslyn can't write pdb debug information files on Mac OS. This means that you can compile C# with Roslyn on MacOS if you want, but if you do you won't be able to debug it. By default Roslyn is disabled on Mac OS and Mono C# 6.0 compiler is used unless you've installed a special package from the [downloads page][downloads] with "Roslyn on Mac OS" support.

Since WebGL doesn't offer any multithreading support, AsyncBridge and Task Parallel Library are not available for this platform. Caller Info attributes are also not available, because their support comes with AsyncBridge library.

AsyncBridge/TPL stuff is also not compatible with Windows Store Application platform (and probably all the platforms that use .Net runtime instead of Mono runtime) due to API differences between the recent versions of .Net Framework and the ancient version of TPL (System.Threading.dll) that comes with AsyncBridge. Namely, you can't use async/await, Caller Info attributes and everything from System.Threading.dll (concurrent collections for example).

WebPlayer platform is not supported and most likely will never be since it is about to be deprecated.


# Making builds from command line #

If you want to build your project from command line the simple way, it works as usual. For example,

        unity.exe -buildWindows64Player <pathname>

However, if you use [Build Player Pipeline](https://docs.unity3d.com/Manual/BuildPlayerPipeline.html), you'll have to take extra steps, because otherwise the old compiler will be used and the build will fail:

1. Make a copy of `CSharpCompilerWrapper.exe` and place it into `/Unity/Editor/Data/Mono/lib/mono/2.0` on Windows or `/Unity.app/Contents/Frameworks/Mono/lib/mono/2.0` on Mac OS X.
2. Rename this copy to `smcs.exe`.
3. Make sure that in the Player Settings the API Compatibility Level option is set to `NET 2.0`.


# Other known issues #

* C# 5.0/6.0 is not compatible with Unity Cloud Build service for obvious reason.

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


* There's a bug in Mono C# 6.0 compiler related to null-conditional operator support (NullConditionalTest.cs):

        int[] array = new[] { 0, 1, 2, 3 };
        int? item = array?[0];
        bool foo = (array?[0]).HasValue; // error CS0266:
        // Cannot implicitly convert type `bool?' to `bool'.
        // An explicit conversion exists (are you missing a cast?)


# License #

All the source code is published under [WTFPL version 2](http://www.wtfpl.net/about/).


# Want to talk about it? #

http://forum.unity3d.com/threads/c-6-0.314297/#post-2108999

   
# Random notes #

* `mcs.exe`, `pdb2mdb.exe` and its dependencies were taken from [Mono 4.4.1.0][mono] installation. pdb2mdb.exe that comes with Unity is not compatible with the assemblies generated with Roslyn compiler.

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
[ngen]: https://msdn.microsoft.com/en-us/library/6t9t5wcf%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
[downloads]:https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/downloads