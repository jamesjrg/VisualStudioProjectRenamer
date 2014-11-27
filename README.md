VisualStudioProjectRenamer
==========================

A script to rename a C# Visual Studio project. It does the following:

    * Renames the project folder
    * Renames the .csproj
    * Alters the referenced csproj in the root .sln file
    * Alters RootNamespace and AssemblyName in the csproj
    * Alters the AssemblyInfo.cs file in the project, if present
    * Alters namespace xxx lines in .cs files within the project
    * Alters using xxx lines in all .cs files in all projects within the directory tree - i.e. if you have multiple projects within the solution, they too will have using statements altered
    * Alters ProjectReference sections in other .csprojs within the directory tree, i.e. alters references to the project within other projects

As long as you have a relatively recent version of Visual Studio installed you can probably have the F# interpreter for running interactive scripts installed somewhere like this:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe

Because it is a script no compilation is necessary, just go to your root solution directory and run:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe -- <old_project_name> <new_project_name>

or if fsi.exe is already in your PATH:

fsi.exe -- <old_project_name> <new_project_name>

No backup is created, because I assume anyone using this is already using source control and can easily revert the changes if it goes wrong.

If it doesn't do exactly what you want then just open the script, fiddle with it to your liking, and run it again. It uses regexes rather than properly parsing XML and C#, because it is simple and it works.