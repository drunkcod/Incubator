open System
open System.Diagnostics
open System.Reflection
open System.IO

let findFakeFile() =
    let defaultFakeFile x = 
        match Path.GetFileName(x) with
        | "fakefile.fsx" -> true
        | _ -> false
    Directory.GetFiles(Environment.CurrentDirectory)
    |> Seq.tryFind defaultFakeFile

[<EntryPoint>]
let main args =
    match findFakeFile() with
    | None -> 
        Console.WriteLine "No Fakefile found (looking for: fakefile.fsx)"
        -1
    | Some(x) ->
        args |> Seq.iter Console.WriteLine
        0
