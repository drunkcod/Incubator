open System
open System.Collections.Generic
open System.DirectoryServices

module DirectoryEntry =
    let children (x:DirectoryEntry) = x.Children |> Seq.cast<DirectoryEntry>
    let rec flatten (x:DirectoryEntry) = seq { 
        yield x 
        yield! x.Children |> Seq.cast<DirectoryEntry> |> Seq.collect flatten }
        

type System.DirectoryServices.DirectoryEntry with   
    member x.PickChild p = 
        DirectoryEntry.flatten x
        |> Seq.pick p
    
    member x.PickParent (p:DirectoryEntry->'a option) =
        let rec loop node = 
            match p(node) with
            | Some(x) -> x
            | None -> loop node.Parent
        loop x.Parent
        
type WebSiteState = 
    | Starting = 1
    | Started = 2
    | Stopping = 3
    | Stopped = 4
    | Pausing = 5
    | Paused = 6
    | Continuing = 7

type IWebSite =
    abstract Name : string
    abstract Children : DirectoryEntries
    abstract Properties : PropertyCollection
    abstract VirtualDirectories : seq<IVirtualDirectory>
    abstract ServerState : WebSiteState
    abstract Start : unit -> unit
    abstract Stop : unit -> unit

and IVirtualDirectory =
    abstract Name : string
    abstract WebSite : IWebSite       

type IisAdmin(path) = 
    let rec AsVirtualDirectory (entry:DirectoryEntry) = 
        if entry.SchemaClassName <> "IIsWebVirtualDir" then 
            None
        else Some({new IVirtualDirectory with 
            member x.Name = entry.Name
            member x.WebSite = entry.PickParent AsWebSite})
            
    and AsWebSite (entry:DirectoryEntry) =
        if entry.SchemaClassName <> "IIsWebSite" then
            None
        else Some({ new IWebSite with
            member x.Name = entry.Path
            member x.Children = entry.Children
            member x.VirtualDirectories =
                DirectoryEntry.flatten entry
                |> Seq.choose AsVirtualDirectory
            member x.Properties = entry.Properties
            member x.ServerState = entry.InvokeGet("ServerState") :?> WebSiteState
            member x.Start() = entry.Invoke("Start") |> ignore
            member x.Stop() = entry.Invoke("Stop") |> ignore })        

    let iis = new DirectoryEntry(path)

    member x.FindVirtualDirectory name = 
        iis.PickChild (fun x -> 
            if System.String.Compare(x.Name, name, true) <> 0 then
                None
            else x |> AsVirtualDirectory)
try
    let iis = IisAdmin("IIS://www1.qb.local/w3svc")
    iis.FindVirtualDirectory "cpx"
    |> fun x -> Console.WriteLine("{0} is {1}", x.WebSite.Name, x.WebSite.ServerState)
with | :? KeyNotFoundException as e -> Console.WriteLine("No such virtual directory")    
