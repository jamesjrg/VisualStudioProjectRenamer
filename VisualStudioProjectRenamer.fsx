open System.IO
open System.Text.RegularExpressions

//Config

let fileTypesToMatchForReplacement = ["*.cs"]

//Definitions inspired by on Scott Wlaschin's railway oriented programming http://fsharpforfunandprofit.com/posts/recipe-part2/

type Result = 
    | Success of string * string
    | Failure of string

let bind func result = 
    match result with
    | Success (oldName, newName) -> func oldName newName
    | Failure f -> Failure f

//Interpreting command line arguments

let parseArgs =
    let args = fsi.CommandLineArgs
    if args.Length > 1 then Success (args.[0], args.[1]) else Failure "couldn't parse command line arguments"

let validateArgs oldName newName =
    match Directory.Exists oldName with
    | true ->
        match Directory.Exists(newName) with
        | true -> Failure (sprintf "Directory already exists: %s" newName)
        | false -> Success (oldName, newName)
    | false -> Failure (sprintf "No such directory: %s" oldName)

// Error handler

let handleError result = 
    match result with
    | Failure errorMsg -> printfn "%s" errorMsg
    | _ -> ()

//The path to the csproj file

let getCsprojPath newName = Path.Combine(newName, newName + ".csproj")

// renaming files and directories

let renameDirectory oldName newName =
    printfn "Renaming directory %s to %s" oldName newName
    try
        Directory.Move(oldName, newName)
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

let renameCsProj oldName newName =
    let oldFileName = Path.Combine(newName, oldName + ".csproj")
    let newFileName = getCsprojPath newName
    printfn "Renaming csproj %s to %s" oldFileName newFileName
    try
        File.Move(oldFileName, newFileName);
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

// modifying files

let rec getFilesForSearch basePath =
    seq {
        for matchPattern in fileTypesToMatchForReplacement do
            yield! Directory.EnumerateFiles(basePath, matchPattern, SearchOption.AllDirectories)
    }

let makeReplacementsInFile filePath (replacements : (string * string) seq) = 
    let oldContents = File.ReadAllText(filePath)     
    let newContents = Seq.fold (fun (state : string) (pattern : string, replacmentText : string) -> state.Replace(pattern, replacmentText)) oldContents replacements

    File.WriteAllText(filePath, newContents)

let regixifyFile filePath pattern replacement =
    let oldContents = File.ReadAllText(filePath)     
    //TODO
    let newContents = ""

    File.WriteAllText(filePath, newContents)

let alterSlnFile oldName newName =
    printfn "Altering sln file"

    let maybeSlnFile = Directory.EnumerateFiles(".", "*.sln") |> Seq.tryPick Some

    match maybeSlnFile with
    | Some slnFilePath -> 
        makeReplacementsInFile slnFilePath [(oldName + ".csproj", newName + ".csproj")]
        Success (oldName, newName)
    | _ -> Failure "No .sln file found"

let alterCsproj oldName newName =
    printfn "Altering .csproj AssemblyName and RootNamespace"
    makeReplacementsInFile (getCsprojPath newName) [
        ("<AssemblyName>" + oldName, "<AssemblyName>" + newName);
        ("<RootNamespace>" + oldName, "<RootNamespace>" + newName);
    ]
    Success (oldName, newName)

let alterAssemblyInfo oldName newName =
    let assemblyInfoPath = Path.Combine [|newName; "Properties"; "AssemblyInfo.cs"|]

    match File.Exists assemblyInfoPath with
    | true ->
        printfn "Altering %s" assemblyInfoPath
        makeReplacementsInFile assemblyInfoPath [(oldName, newName)];
        Success (oldName, newName)
    | false ->
        printfn "No AssemblyInfo.cs found, skipping step"
        Success (oldName, newName)

let fileContainsRegex filePath pattern =
    let contents = File.ReadAllText(filePath)
    Regex.IsMatch(contents, pattern)

let getFilesForModification newName regexMatch =
    let filesForSearch = getFilesForSearch newName
    Seq.filter (fun file -> fileContainsRegex file regexMatch) filesForSearch

let alterNamespaces oldName newName =
    printfn "Altering namespaces"
    let regexMatch = "(\s*using )" + oldName;
    let regexReplace = "\1" + newName;
    let filesForModification = getFilesForModification newName regexMatch
    for file in filesForModification do regixifyFile file regexMatch regexReplace
    Success (oldName, newName)

let alterUsings oldName newName =
    printfn "Altering usings"
    //TODO
    Success (oldName, newName)

let alterProjectReferences oldName newName =
    printfn "Altering project references"
    //TODO
    Success (oldName, newName)

parseArgs 
|> bind validateArgs
|> bind renameDirectory
|> bind renameCsProj
|> bind alterSlnFile
|> bind alterCsproj
|> bind alterAssemblyInfo
|> bind alterNamespaces
|> bind alterUsings
|> bind alterProjectReferences
|> handleError

