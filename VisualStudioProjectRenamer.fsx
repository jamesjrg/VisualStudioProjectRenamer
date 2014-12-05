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

// renaming files and directories

let renameDirectory oldName newName =
    try
        Directory.Move(oldName, newName)
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

let renameCsProj oldName newName =
    try
        File.Move(oldName + ".csproj", newName + ".csproj");
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

// modifying files

let rec getFilesForModification basePath =
    seq {
        for matchPattern in fileTypesToMatchForReplacement do
            yield! Directory.EnumerateFiles(basePath, matchPattern, SearchOption.AllDirectories)
    }

let alterSlnFile oldName newName =
    let maybeSlnFile = Directory.EnumerateFiles(".", "*.sln") |> Seq.tryPick Some
    match maybeSlnFile with
    | Some slnFile -> 
        let oldSlnContents = File.ReadAllText(slnFile) 
        let newSlnFileContents = oldSlnContents.Replace(oldName + ".csproj", newName + ".csproj")
        File.WriteAllText(@"C:\Users\Public\TestFolder\WriteText.txt", newSlnFileContents)
        Success (oldName, newName)
    | _ -> Failure "No .sln file found"

let alterCsproj oldName newName =
    let oldSlnContents = File.ReadAllText(newName + ".csproj") 
    Success (oldName, newName)

let alterAssemblyInfo oldName newName =
    Success (oldName, newName)

let alterNamespaces oldName newName =
    Success (oldName, newName)

let alterUsings oldName newName =
    Success (oldName, newName)

let alterProjectReferences oldName newName =
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