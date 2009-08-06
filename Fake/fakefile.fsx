#r "Lib\\Fake.dll"

open System
open System.Reflection
open Fake

[<Literal>]
let TypePrefix = "FSI_0001+"

let invoke (taskName:string) =
    let parts = taskName.Split('.')
    let t = Type.GetType(TypePrefix + parts.[0], true)
    let prop = t.GetProperty(parts.[1], BindingFlags.Static + BindingFlags.Public + BindingFlags.NonPublic)
    (prop.GetValue(null, null) :?> ITask).Run()

module Build =
    let compile = task (fun () ->
        let steps = [|
            "fsc --nologo Source\Fake.fs Source\Shell.fs --target:library -o Build\Fake.dll";
            "echo Done."|]
        steps |> Seq.iter Shell.run)
try
    invoke "Build.compile"
with 
    Failure e -> Console.WriteLine e