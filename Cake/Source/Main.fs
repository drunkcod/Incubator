namespace Cake
open System
open System.IO
open System.CodeDom.Compiler
open Microsoft.FSharp.Compiler.CodeDom

module CakeConsole =
    let findCakeFile() =
        let defaultCakeFile x = 
            match Path.GetFileName(x) with
            | "Cakefile.fs" -> true
            | _ -> false
        Directory.GetFiles(Environment.CurrentDirectory)
        |> Seq.tryFind defaultCakeFile

    let compile path =
        use compiler = new FSharpCodeProvider() 
        let options = CompilerParameters(GenerateExecutable = false, GenerateInMemory = true)
        options.ReferencedAssemblies.Add(typeof<option<_>>.Assembly.Location) |> ignore
        options.ReferencedAssemblies.Add(typeof<CakeBuild>.Assembly.Location) |> ignore
        compiler.CompileAssemblyFromFile(options, [|path|])

    let CakeFileNotFound() =
        Console.WriteLine "No Cakefile found (looking for: Cakefile.fs)"
        -1

    let tryInvoke (result:CompilerResults) args=        
        if result.NativeCompilerReturnValue <> 0 then
            result.Errors |> Seq.cast<CompilerError> |> Seq.iter Console.WriteLine
            -1
        else            
            let result =
                let Cake = CakeBuild(result.CompiledAssembly) 
                Cake.MissingTarget <- fun target -> Console.WriteLine("Target: \"{0}\" not found.", target)

                args |> Seq.map Cake.Invoke
                |> Seq.forall (fun x -> x = Status.Ok)
            if result then
                0
            else -1
                
    [<EntryPoint>]
    let main args =
        match findCakeFile() |> Option.map compile with
        | None -> CakeFileNotFound()
        | Some(result) ->  tryInvoke result args
