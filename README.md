VisualStudioProjectRenamer
==========================

A relatively simple script to rename a C# Visual Studio project. It does the following:

    * Renames the project folder
    
    * Renames the .csproj
    
    * Alters the referenced csproj in the root .sln file
    
    * Alters RootNamespace and AssemblyName in the csproj
    
    * Alters the AssemblyInfo.cs file in the project, if present
    
    * Alters namespace declarations in files matching a given pattern within the project directory. It will only rewrite files that need modification - it won't change last modified timestamps on unaffected files.
    
    * Alters using statements in files matching a given pattern in all projects within the directory tree. If you have multiple projects within the solution, they too will have using statements altered. It will only rewrite files that need modification - it won't change last modified timestamps on unaffected files.
    
    * Alters ProjectReference sections in other .csprojs within the directory tree, i.e. alters references to the project within other projects

Notes:

    * By default the file matching pattern for finding using and namespace declarations is "*.cs". You can alter this by changing the fileTypesToMatchForReplacement array at the top of the script, but it will only search for C#-style using and namespace syntax.
    
    * No backup is created, because I assume anyone using this is already using source control and can easily revert the changes if it goes wrong.
    
    * It assumes that you will run it in the solution folder, and that the project to be renamed is in a subfolder with the same name as the project
    
    * It assumes there is only a single .sln file in the directory where the script is run

    * It won't fix qualified references, e.g. var x = ProjectToBeRenamed.MyClass;
    
If it doesn't do quite what you want then just open the script, fiddle with it to your liking, and run it again.

How to run it:

As long as you have a relatively recent version of Visual Studio installed you can probably have the F# interpreter for running interactive scripts installed somewhere like this:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe

Because it is a script no compilation is necessary, just go to your root solution directory and run:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe [PATH_TO_THE_SCRIPT]VisualStudioProjectRenamer.fsx -- [old_project_name] [new_project_name]

or if fsi.exe is already in your PATH:

fsi.exe [PATH_TO_THE_SCRIPT]VisualStudioProjectRenamer.fsx -- [old_project_name] [new_project_name]


