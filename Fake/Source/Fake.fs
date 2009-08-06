namespace Fake

type ITask =
    abstract Run : unit -> unit

[<AutoOpen>]
module Fake =
    let task x = 
        {new obj() 
            interface ITask 
            with member this.Run() = x()}
        
    let run (task:ITask) = task.Run()