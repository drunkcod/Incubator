namespace Fake
open System
open System.Reflection
open Xlnt.Stuff

type Status = 
    | Ok = 0
    | TargetFailed = 1
    | TargetMissing = 2

type FakeBuild(assembly:Assembly) =                     
    let mutable missingTarget : string -> unit = fun x -> ()
    
    member this.MissingTarget
        with get() = missingTarget
        and set(value:string -> unit) = missingTarget <- value

    member private this.FindTargetType name =
        try
            assembly.GetTypes()
            |> Seq.tryFind (fun x -> x.Name = name)
        with :? ReflectionTypeLoadException as e ->
            e.LoaderExceptions
            |> Seq.iter Console.WriteLine
            None
        
    static member private GetProperty (name:string) (t:Type) =
        t.GetProperty(name, BindingFlags.Static + BindingFlags.Public + BindingFlags.NonPublic)
        |> Option.maybe
    
    static member private GetValue (name:string) (t:Type option) =
        Option.bind (FakeBuild.GetProperty name) t
        |> Option.map (fun prop -> prop.GetValue(null, null))
     
    member private this.GetTask (taskName:string) =
        let parts = taskName.Split('.')
        this.FindTargetType parts.[0]
        |> FakeBuild.GetValue parts.[1] 
                                    
    member this.Invoke (taskName:string) =
        executed.Clear()
        match this.GetTask taskName with
        | None -> 
            this.MissingTarget taskName
            Status.TargetMissing
        | Some(target) ->
            try
             (target :?> ITarget).Run()
             Status.Ok
            with _ -> Status.TargetFailed