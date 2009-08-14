namespace Fake
open System
open System.Collections.Generic
open System.Reflection

type Status = 
    | Ok = 0
    | TargetFailed = 1
    | TargetMissing = 2

[<AutoOpen>]
module Fake =
    let private executed = HashSet<ITarget>()

    let run (target:ITarget) = target.Run()
       
    let task x = 
        {new obj() 
            interface ITarget 
            with member this.Run() = x()}

    let (=>) required x = task (fun () ->
        required |> Seq.iter (fun item ->
            if executed.Add(item) then
                run item)   
        run x)
            
    let exec prepare = 
        let shell = ShellTask()
        prepare shell
        shell            
         
    let private findTargetType (assembly:Assembly) name =
        assembly.GetTypes()
        |> Seq.tryFind (fun x -> x.Name = name)   
        
    let private maybe = function
        | null -> None
        | x -> Some(x) 
        
    let private getProperty (name:string) (t:Type) =
        maybe(t.GetProperty(name, BindingFlags.Static + BindingFlags.Public + BindingFlags.NonPublic))
    
    let private getValue (name:string) (t:Type option) =
        Option.bind (getProperty name) t
        |> Option.map (fun prop -> prop.GetValue(null, null))
     
    let private getTask (assembly:Assembly) (taskName:string) =
        let parts = taskName.Split('.')
        findTargetType assembly parts.[0]
        |> getValue parts.[1] 
        
    let mutable MissingTarget = fun x -> ()        
                            
    let invoke (assembly:Assembly) (taskName:string) =
        executed.Clear()
        match getTask assembly taskName with
            | None -> 
                MissingTarget taskName
                Status.TargetMissing
            | Some(target) ->
                try
                 (target :?> ITarget).Run()
                 Status.Ok
                with _ -> Status.TargetFailed