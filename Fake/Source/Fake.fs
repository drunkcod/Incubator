namespace Fake
open System
open System.Collections.Generic
open System.Reflection

type ShellTask() =
    [<DefaultValue>] val mutable Program : string
    [<DefaultValue>] val mutable Arguments : string

    interface ITarget with
        member this.Run() = Shell.Run(this.Program + " " + this.Arguments)

[<AutoOpen>]
module Fake =
    let private executed = HashSet<ITarget>()

    let run (target:ITarget) = target.Run()
       
    let (=>) required x = 
        {new obj()
            interface ITarget with
                member this.Run() =
                    required |> Seq.iter (fun item ->
                    if executed.Add(item) then
                        run item)   
                    run x}        
    let task x = 
        {new obj() 
            interface ITarget 
            with member this.Run() = x()}
            
    let exec prepare = 
        let shell = ShellTask()
        prepare shell
        shell            
         
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
        executed.Clear()
        match getTask assembly taskName with
            | None -> failwith ("target \"" + taskName + "\" not found.")
            | Some(prop) -> (prop.GetValue(null, null) :?> ITarget).Run()