open System
open System.IO
open System.CodeDom.Compiler
open Microsoft.FSharp.Compiler.CodeDom
open Fake

let findFakeFile() =
    let defaultFakeFile x = 
        match Path.GetFileName(x) with
        | "fakefile.fs" -> true
        | _ -> false
    Directory.GetFiles(Environment.CurrentDirectory)
    |> Seq.tryFind defaultFakeFile

let compile path =
    use compiler = new FSharpCodeProvider()
    let options = CompilerParameters(GenerateExecutable = false, GenerateInMemory = true)
    options.ReferencedAssemblies.Add(typeof<Fake.ITarget>.Assembly.Location) |> ignore
    compiler.CompileAssemblyFromFile(options, [|path|])

let fakeFileNotFound() =
    Console.WriteLine "No Fakefile found (looking for: fakefile.fs)"
    -1

let tryInvoke (result:CompilerResults) args=        
    if result.NativeCompilerReturnValue <> 0 then
        result.Errors |> Seq.cast<CompilerError> |> Seq.iter Console.WriteLine
        -1
    else            
        args |> Seq.iter (invoke result.CompiledAssembly)
        0
            
[<EntryPoint>]
let main args =
    match findFakeFile() |> Option.map compile with
    | None -> fakeFileNotFound()
    | Some(result) ->  tryInvoke result args
