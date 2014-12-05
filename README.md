VisualStudioProjectRenamer
==========================

A script to rename a C# Visual Studio project. It does the following:

    * Renames the project folder
    
    * Renames the .csproj
    
    * Alters the referenced csproj in the root .sln file
    
    * Alters RootNamespace and AssemblyName in the csproj
    
    * Alters the AssemblyInfo.cs file in the project, if present
    
    * Alters namespace xxx lines in files matching a given pattern, within the project. It will make some attempt not to alter comments that happen to use the phrase "namespace", but it's still a simple regex and could fail.
    
    * Alters using xxx lines in files matching a given pattern, in all projects within the directory tree - i.e. if you have multiple projects within the solution, they too will have using statements altered. It will make some attempt not to alter comments that happen to use the phrase "using", but it's still a simple regex and could fail.
    
    * Alters ProjectReference sections in other .csprojs within the directory tree, i.e. alters references to the project within other projects

Notes:

    * By default the file matching pattern is "*.cs". You can alter this by changing the fileTypesToMatchForReplacement array at the top of the script.
    
    * No backup is created, because I assume anyone using this is already using source control and can easily revert the changes if it goes wrong.
    
    * It assumes that you will run it in the solution folder, and that the project to be renamed is in a subfolder with the same name as the project
    
    * It assumes there is only a single .sln file in the directory where the script is run
    
If it doesn't do quite what you want then just open the script, fiddle with it to your liking, and run it again.

How to run it:

As long as you have a relatively recent version of Visual Studio installed you can probably have the F# interpreter for running interactive scripts installed somewhere like this:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe

Because it is a script no compilation is necessary, just go to your root solution directory and run:

C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe -- <old_project_name> <new_project_name>

or if fsi.exe is already in your PATH:

fsi.exe -- <old_project_name> <new_project_name>


