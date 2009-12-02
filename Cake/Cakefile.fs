namespace Cake
open Cake
open System
open System.IO
open System.Runtime.InteropServices
    
module Build =
    let CompileCore = exec <| fun t ->        
        t.Program <- "fsc"
        t.Arguments <- "-a -o Build\Cake.Core.dll --nologo Source\\ITarget.fs Source\\Shell.fs Source\\ShellTask.fs Source\\CakeTasks.fs Source\\Cake.fs -r System.Core"
            
    let CompileExe = [CompileCore] => exec (fun t ->
        t.Program <- "fsc"        
        t.Arguments <- "--target:exe -o Build\Cake.exe --nologo -r Lib\\FSharp.Compiler.CodeDom.dll -r Lib\\Cake.Core.dll Source\\Main.fs")
    
    let MSBuild = msbuild "Cake.sln"

    let compile = [CompileExe] => task (fun () ->
        Console.WriteLine "Done.")                        