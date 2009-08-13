open Fake
open System
    
module Build =
    let CompileCore = exec <| fun t ->
        t.Program <- "fsc"
        t.Arguments <- "-a -o Build\Fake.Core.dll --nologo Source\\ITarget.fs Source\\Shell.fs Source\\Fake.fs -r System.Core"
            
    let CompileExe = [CompileCore] => exec (fun t ->
        t.Program <- "fsc"        
        t.Arguments <- "--target:exe -o Build\Fake.exe --nologo -r Lib\\FSharp.Compiler.CodeDom.dll -r Lib\\Fake.Core.dll Source\\Main.fs")

    let compile = [CompileExe] => task (fun () ->
        Console.WriteLine "Done.")                        