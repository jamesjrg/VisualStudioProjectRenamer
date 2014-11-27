open System.IO

let parseArgs =
    let args = fsi.CommandLineArgs
    if args.Length > 1 then Some (args.[0], args.[1]) else None

let validateArgs oldName newName : string option =
     match Directory.Exists oldName with
     | true ->
        match Directory.Exists(newName) with
        | true -> Some (sprintf "Directory already exists: %s" newName)
        | false -> None 
     | false -> Some (sprintf "No such directory: %s" oldName)

let renameDirectory oldName newName =
    ()

let renameCsProj oldName newName =
    ()

let alterSlnFile oldName newName =
    ()

let alterCsproj oldName newName =
    ()

let alterAssemblyInfo oldName newName =
    ()

let alterNamespaces oldName newName =
    ()

let alterUsings oldName newName =
    ()

let alterProjectReferences oldName newName =
    ()

let doRename oldName newName =
    renameDirectory oldName newName
    renameCsProj oldName newName
    alterSlnFile oldName newName
    alterCsproj oldName newName
    alterAssemblyInfo oldName newName
    alterNamespaces oldName newName
    alterUsings oldName newName
    alterProjectReferences oldName newName

match parseArgs with
| Some (oldName, newName) ->
    match validateArgs oldName newName with    
    | Some(errorMsg) -> printfn "%s" errorMsg
    | None -> doRename oldName newName
| None -> ()