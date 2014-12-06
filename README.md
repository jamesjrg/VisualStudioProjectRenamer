VisualStudioProjectRenamer
==========================

A script to rename a Visual Studio project. The default settings assume a fairly simple C# project, but by changing the configuration settings and/or modifying the script it should work for other languages and more unusual cases. It does the following:

    * Renames the project folder and the project file within it
    
    * Alters the referenced project in the root .sln file
    
    * Runs a set of replacements on the contents of the project file - by default RootNamespace and AssemblyName
    
    * Alters the AssemblyInfo.cs file (if present)
    
    * Runs a set of regex replacements on files within the project directory which have a certain extension. By default the only replacement carried out at this stage is for C#-style namespace declarations in .cs files, but this can be modified in the config section.

    * Runs a set of regex replacements on all files within the solution directory which have a certain extension. By default the only replacement carried out at this stage is for C#-style using declarations in .cs files, but this can be modified in the config section.
    
    * Alters references to the project within other projects in the solution. By default this means modifying ProjectReference sections in .csproj files.

Notes:

    * You can modify the types of files modified, the modifications carried out and various other things in the config section at the top of the file.

    * It will only rewrite files that need modification - it won't change last modified timestamps on unaffected files.
    
    * No backup is created, because I assume anyone using this is already using source control and can easily revert the changes if it goes wrong.
    
If it doesn't do quite what you want then just open the script, fiddle with it to your liking, and run it again.

How to run it:

As long as you have a relatively recent version of Visual Studio installed you can probably already have the F# interpreter for running interactive scripts installed, and because it is a script no compilation is necessary. Put this script in the root of your solution directory and then run something along the lines of:

"C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\Fsi.exe" VisualStudioProjectRenamer.fsx -- BadlyNamedProject LovelyNewName

If your Fsi.exe is located somewhere else then modify the above appropriately.