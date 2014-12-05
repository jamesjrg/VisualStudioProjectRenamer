open System.IO

//Config

let fileTypesToMatchForReplacement = [|"*.cs"|]

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

let rec getFilesForModification basePath =
    seq {
        for matchPattern in fileTypesToMatchForReplacement do
            yield! Directory.EnumerateFiles(basePath, matchPattern, SearchOption.AllDirectories)
    }

let modifyFile filePath (replacements : (string * string) seq) = 
    let oldContents = File.ReadAllText(filePath)     
    let newContents = Seq.fold (fun (state : string) (pattern : string, replacmentText : string) -> state.Replace(pattern, replacmentText)) oldContents replacements

    File.WriteAllText(filePath, newContents)

let alterSlnFile oldName newName =
    printfn "Altering sln file"

    let maybeSlnFile = Directory.EnumerateFiles(".", "*.sln") |> Seq.tryPick Some

    match maybeSlnFile with
    | Some slnFilePath -> 
        modifyFile slnFilePath [(oldName + ".csproj", newName + ".csproj")]
        Success (oldName, newName)
    | _ -> Failure "No .sln file found"

let alterCsproj oldName newName =
    printfn "Altering .csproj AssemblyName and RootNamespace"
    let filename = getCsprojPath newName
    //TODO  
    //modifyFile    
    Success (oldName, newName)

let alterAssemblyInfo oldName newName =
    let assemblyInfoPath = Path.Combine [|newName; "Properties"; "AssemblyInfo.cs"|]

    match File.Exists assemblyInfoPath with
    | true ->
        printfn "Altering %s" assemblyInfoPath
        //TODO
        Success (oldName, newName)
    | false ->
        printfn "No AssemblyInfo.cs found, skipping step"
        Success (oldName, newName)   

let alterNamespaces oldName newName =
    printfn "Altering namespaces"
    Success (oldName, newName)

let alterUsings oldName newName =
    printfn "Altering usings"
    Success (oldName, newName)

let alterProjectReferences oldName newName =
    printfn "Altering project references"
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