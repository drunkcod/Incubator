#r "Lib\\Fake.dll"

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
            "fsc --nologo Source\Fake.fs Source\Shell.fs --target:library -o Build\Fake.dll";
            "echo Done."|]
        steps |> Seq.iter Shell.run)


invoke "Build.compile"
invoke "Build.test"
invoke "Missing.thing"