namespace Fake
open System
open System.IO
open System.CodeDom.Compiler
open Microsoft.FSharp.Compiler.CodeDom

module FakeConsole =
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
        options.ReferencedAssemblies.Add(typeof<option<_>>.Assembly.Location) |> ignore
        options.ReferencedAssemblies.Add(typeof<FakeBuild>.Assembly.Location) |> ignore
        compiler.CompileAssemblyFromFile(options, [|path|])

    let fakeFileNotFound() =
        Console.WriteLine "No Fakefile found (looking for: fakefile.fs)"
        -1

    let tryInvoke (result:CompilerResults) args=        
        if result.NativeCompilerReturnValue <> 0 then
            result.Errors |> Seq.cast<CompilerError> |> Seq.iter Console.WriteLine
            -1
        else            
            let result =
                let fake = FakeBuild(result.CompiledAssembly) 
                fake.MissingTarget <- fun target -> Console.WriteLine("Target: \"{0}\" not found.", target)

                args |> Seq.map fake.Invoke
                |> Seq.forall (fun x -> x = Status.Ok)
            if result then
                0
            else -1
                
    [<EntryPoint>]
    let main args =
        match findFakeFile() |> Option.map compile with
        | None -> fakeFileNotFound()
        | Some(result) ->  tryInvoke result args
