namespace Fake
open System
open System.Reflection

type ITask =
    abstract Run : unit -> unit

[<AutoOpen>]
module Fake =
    let task x = 
        {new obj() 
            interface ITask 
            with member this.Run() = x()}
        
    let run (task:ITask) = task.Run()
     
    let private findTargetType (assembly:Assembly) name =
        assembly.GetTypes()
        |> Seq.tryFind (fun x -> x.Name = name)   
        
    let private getProperty (t:Type option) (name:string) =
        match t with
            None -> None
            | Some(t) ->
                let prop = t.GetProperty(name, BindingFlags.Static + BindingFlags.Public + BindingFlags.NonPublic)
                if prop = null then
                    None
                else Some(prop)                    
     
    let private getTask (assembly:Assembly) (taskName:string) =
        let parts = taskName.Split('.')
        let targetType = findTargetType assembly parts.[0]
        getProperty targetType parts.[1]
                            
    let invoke (assembly:Assembly) (taskName:string) =
        match getTask assembly taskName with
            | None -> failwith ("target \"" + taskName + " not found.")
            | Some(prop) -> (prop.GetValue(null, null) :?> ITask).Run()