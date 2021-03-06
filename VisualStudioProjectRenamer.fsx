open System.IO
open System.Text.RegularExpressions

(* config *)

//the path to the .sln file - by default the first .sln file found in the current directory
let maybeSlnFile = Directory.EnumerateFiles(".", "*.sln") |> Seq.tryPick Some

//The path to the old and new project files
let oldProjectFilePath oldName newName = Path.Combine(newName, oldName + ".csproj") //this is after folder has already been renamed
let newProjectFilePath newName = Path.Combine(newName, newName + ".csproj")

//Project files in which references should be updated
//to include sub-sub folders: Directory.EnumerateFiles(".", "*.csproj", SearchOption.AllDirectories)
let otherProjectFiles = seq {
    for subDir in Directory.GetDirectories(".") do
        yield! Directory.EnumerateFiles(subDir, "*.csproj")
}

//in which file types should the regex modifications be carried out
let fileTypesToMatchForReplacement = ["*.cs"]

//Modifications for the project file
//A list of tuples, the first item is a simple non-regex string to match, the second item the replacement text
let projectFileReplacements  oldName newName = [
    ("<AssemblyName>" + oldName, "<AssemblyName>" + newName);
    ("<RootNamespace>" + oldName, "<RootNamespace>" + newName);
]

//Modifications for files within the project being renamed
//A list of tuples, the first item is the regex to match, the second item the replacement text
let modificationsForFilesInProject oldName newName = [
    ("(\s*namespace )" + oldName, "$1" + newName);
]

//Modifications for files within the solution
//A list of tuples, the first item is the regex to match, the second item the replacement text
let modificationsForFilesInSolution oldName newName = [
    ("(\s*using )" + oldName, "$1" + newName);
]

//Modifications for altering project references in other projects
//A list of tuples, the first item is the regex to match, the second item the replacement text
let modificationsForProjectReferences oldName newName = [
    (sprintf @"%s\\%s\.csproj" oldName oldName, sprintf @"%s\\%s\.csproj" newName newName);
    (sprintf "<Name>%s</Name>" oldName, sprintf "<Name>%s</Name>" newName);
]

(* end config *)

(* Definitions inspired by Scott Wlaschin's post on railway oriented programming http://fsharpforfunandprofit.com/posts/recipe-part2/
*)

type Result = 
    | Success of string * string
    | Failure of string

let bind func result = 
    match result with
    | Success (oldName, newName) -> func oldName newName
    | Failure f -> Failure f

let (>>=) twoTrackInput switchFunction = 
    bind switchFunction twoTrackInput

(* end railways *)

(* interpreting command line arguments *)

//point free, not really sure if it makes things too unreadable
let rec getArgs = function
    | "--" :: tail -> List.toArray tail
    | _ :: tail -> getArgs tail
    | [] -> Array.empty

let parseArgs =
    let args = fsi.CommandLineArgs |> Array.toList |> getArgs
    if args.Length > 1 then Success (args.[0], args.[1]) else Failure "couldn't parse command line arguments"

let validateArgs oldName newName =
    match Directory.Exists oldName with
    | true ->
        match Directory.Exists(newName) with
        | true -> Failure (sprintf "Directory already exists: %s" newName)
        | false -> Success (oldName, newName)
    | false -> Failure (sprintf "No such directory: %s" oldName)

(* end arguing *)

(* error handler *)

let handleError result = 
    match result with
    | Failure errorMsg -> printfn "%s" errorMsg
    | _ -> ()

(* end errors *)

(* renaming files and directories *)

let renameProjectDirectory oldName newName =
    printfn "Renaming directory %s to %s" oldName newName
    try
        Directory.Move(oldName, newName)
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

let renameProjectFile oldName newName =
    let oldFileName = oldProjectFilePath oldName newName
    let newFileName = newProjectFilePath newName
    printfn "Renaming project file %s to %s" oldFileName newFileName
    try
        File.Move(oldFileName, newFileName);
        Success (oldName, newName)
    with
        | ex -> Failure ex.Message

(* end renaming *)

(* general file and directory manipulation functions *)

let rec getFilesForSearch basePath =
    seq {
        for matchPattern in fileTypesToMatchForReplacement do
            yield! Directory.EnumerateFiles(basePath, matchPattern, SearchOption.AllDirectories)
    }

let makeSimpleReplacementsInFile filePath (replacements : (string * string) seq) = 
    let oldContents = File.ReadAllText(filePath)     
    let newContents = Seq.fold (fun (state : string) (pattern : string, replacmentText : string) -> state.Replace(pattern, replacmentText)) oldContents replacements

    File.WriteAllText(filePath, newContents)

let makeRegexReplacementsInFile filePath (replacements : (string * string) seq) = 
    let oldContents = File.ReadAllText(filePath)     
    let newContents = Seq.fold (fun (state : string) (pattern : string, replacmentText : string) -> Regex.Replace(state, pattern, replacmentText)) oldContents replacements

    File.WriteAllText(filePath, newContents)

let fileContainsRegex filePath pattern =
    let contents = File.ReadAllText(filePath)
    Regex.IsMatch(contents, pattern)

(* end manipulation *)

(* everything else *)

let modifySolutionFile oldName newName =
    printfn "Modifying sln file"

    match maybeSlnFile with
    | Some slnFilePath -> 
        makeSimpleReplacementsInFile slnFilePath [(oldName + ".csproj", newName + ".csproj")]
        Success (oldName, newName)
    | _ -> Failure "No .sln file found"

let modifyProjectFile oldName newName =
    printfn "Modifying project file"
    makeSimpleReplacementsInFile (newProjectFilePath newName) (projectFileReplacements  oldName newName)
    Success (oldName, newName)

let maybeModifyAssemblyInfo oldName newName =
    let assemblyInfoPath = Path.Combine [|newName; "Properties"; "AssemblyInfo.cs"|]

    match File.Exists assemblyInfoPath with
    | true ->
        printfn "Modifying %s" assemblyInfoPath
        makeSimpleReplacementsInFile assemblyInfoPath [(oldName, newName)];
        Success (oldName, newName)
    | false ->
        printfn "No AssemblyInfo.cs found, skipping step"
        Success (oldName, newName)

let fileHasMatch file modifications =
    Seq.exists (fun replacementTuple -> fileContainsRegex file <| fst replacementTuple) modifications

let getFilesForModificationInProject modifications newName =
    let filesForSearch = getFilesForSearch newName    
    let matchFunc file = fileHasMatch file modifications

    Seq.filter matchFunc filesForSearch

let getFilesForModificationInSolution modifications =
    let filesForSearch = getFilesForSearch "."
    let matchFunc file = fileHasMatch file modifications

    Seq.filter matchFunc filesForSearch

let getOtherProjectFilesForModification modifications = 
    let matchFunc file = fileHasMatch file modifications

    Seq.filter matchFunc otherProjectFiles    

let modifyFilesInProject oldName newName =
    printfn "Modifying files in project (e.g. namespace declarations)"

    let modifications = modificationsForFilesInProject oldName newName
    let filesForModification = getFilesForModificationInProject modifications newName

    for file in filesForModification do makeRegexReplacementsInFile file modifications
    Success (oldName, newName)

let modifyFilesInSolution oldName newName =
    printfn "Modifying files in solution (e.g. using declarations)"

    let modifications = modificationsForFilesInSolution oldName newName
    let filesForModification = getFilesForModificationInSolution modifications

    for file in filesForModification do makeRegexReplacementsInFile file modifications
    Success (oldName, newName)

let modifyProjectReferences oldName newName =
    printfn "Modifying project references"

    let modifications = modificationsForProjectReferences oldName newName    
    let filesForModification = getOtherProjectFilesForModification modifications

    for file in filesForModification do makeRegexReplacementsInFile file modifications
    Success (oldName, newName)

parseArgs 
>>= validateArgs
>>= renameProjectDirectory
>>= renameProjectFile
>>= modifySolutionFile
>>= modifyProjectFile
>>= maybeModifyAssemblyInfo
>>= modifyFilesInProject
>>= modifyFilesInSolution
>>= modifyProjectReferences
|> handleError

