namespace Fake
open Fake
open System
open System.IO
open System.Runtime.InteropServices
    
module Build =
    let CompileCore = exec <| fun t ->        
        t.Program <- "fsc"
        t.Arguments <- "-a -o Build\Fake.Core.dll --nologo Source\\ITarget.fs Source\\Shell.fs Source\\ShellTask.fs Source\\FakeTasks.fs Source\\Fake.fs -r System.Core"
            
    let CompileExe = [CompileCore] => exec (fun t ->
        t.Program <- "fsc"        
        t.Arguments <- "--target:exe -o Build\Fake.exe --nologo -r Lib\\FSharp.Compiler.CodeDom.dll -r Lib\\Fake.Core.dll Source\\Main.fs")
    
    let MSBuild = msbuild "Fake.sln"

    let compile = [CompileExe] => task (fun () ->
        Console.WriteLine "Done.")                        