#r "Lib\\Fake.Core.dll"

open System
open System.Diagnostics
open Fake

let invoke (taskName:string) = 
    let targetAssembly = StackTrace(false).GetFrame(0).GetMethod().DeclaringType.Assembly
    try
        Fake.invoke targetAssembly taskName
    with Failure e -> Console.WriteLine e
    
module Build =
    let compile = task (fun () ->
        let steps = [|
            "fsc -a -o Build\Fake.Core.dll --nologo Source\\Fake.fs Source\\Shell.fs";
            "fsc --target:exe -o Build\Fake.exe --nologo Source\\Main.fs";
            "echo Done."|]
        steps |> Seq.iter Shell.run)

invoke "Build.compile"
invoke "Build.test"
invoke "Missing.thing"
