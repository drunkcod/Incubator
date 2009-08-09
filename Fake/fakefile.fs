open Fake
    
module Build =
    let compile = task (fun () ->
        let steps = [|
            "fsc -a -o Build\Fake.Core.dll --nologo Source\\Fake.fs Source\\Shell.fs";
            "fsc --target:exe -o Build\Fake.exe --nologo -r Lib\\FSharp.Compiler.CodeDom.dll -r Lib\\Fake.Core.dll Source\\Main.fs";
            "echo Done."|]
        steps |> Seq.iter Shell.run)
