namespace Cake
open System.Collections.Generic
open System.IO
open System.Runtime.InteropServices

[<AutoOpen>]
module CakeTasks =        
    let internal executed = HashSet<ITarget>()
           
    let task x = 
        {new obj() 
            interface ITarget 
            with member this.Run() = x()}

    let exec prepare = 
        let shell = ShellTask()
        prepare shell
        shell  

    let msbuild file = 
        ShellTask(
            Program = Path.Combine(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..\\..\\Framework\\v3.5"), "MSBuild.exe"),
            Arguments = file)        

    let private runOnce target =
        if executed.Add(target) then
            Target.run target
            
    let (=>) required x = task <| fun () ->
        required |> Seq.iter runOnce
        Target.run x

