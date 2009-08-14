namespace Fake
open System.Collections.Generic

[<AutoOpen>]
module FakeTasks =        
    let internal executed = HashSet<ITarget>()
           
    let task x = 
        {new obj() 
            interface ITarget 
            with member this.Run() = x()}

    let exec prepare = 
        let shell = ShellTask()
        prepare shell
        shell  

    let private runOnce target =
        if executed.Add(target) then
            Target.run target
            
    let (=>) required x = task <| fun () ->
        required |> Seq.iter runOnce
        Target.run x

