namespace Fake

type ITarget =
    abstract Run : unit -> unit
    
module Target =
    let run (x:ITarget) = x.Run()    