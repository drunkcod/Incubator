namespace Cake
open Cake
open System
open System.IO
open System.Runtime.InteropServices
    
module Build =
    let MSBuild = msbuild "Cake.sln"

    let Compile = [MSBuild] => task (fun () ->
        Console.WriteLine "Done.")                        