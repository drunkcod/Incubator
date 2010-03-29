namespace Xlnt.Stuff
open System
open System.Collections.Generic
open System.DirectoryServices

type SerivceStatus = 
    | Unavailable = 0
    | Starting = 1
    | Started = 2
    | Stopping = 3
    | Stopped = 4
    | Pausing = 5
    | Paused = 6
    | Continuing = 7

type IWebSite =
    abstract Id : string
    abstract Name : string
    abstract Children : DirectoryEntries
    abstract Properties : PropertyCollection
    abstract VirtualDirectories : seq<IVirtualDirectory>
    abstract Status : SerivceStatus
    abstract Start : unit -> unit
    abstract Stop : unit -> unit

and IVirtualDirectory =
    abstract Name : string
    abstract WebSite : IWebSite       

type private DirectoryEntryVirtualDirectoryAdapater(entry:DirectoryEntry) =
    interface IVirtualDirectory with
        member x.Name = entry.Name
        member x.WebSite = entry.PickParent IisAdmin.AsWebSite
    override x.ToString() = entry.Path        

and IisAdmin(path) = 
    let iis = new DirectoryEntry(path)

    static member private Materialize (entry:DirectoryEntry): obj option =
        match entry.SchemaClassName with
        | "IIsWebVirtualDir" -> Some(DirectoryEntryVirtualDirectoryAdapater(entry) :> obj)
        | "IIsWebServer" -> Some({ new IWebSite with
            member x.Id = entry.Name
            member x.Name = entry.InvokeGet("ServerComment") :?> string
            member x.Children = entry.Children
            member x.VirtualDirectories =
                DirectoryEntry.flatten entry
                |> Seq.choose IisAdmin.AsVirtualDirectory
            member x.Properties = entry.Properties
            member x.Status = entry.InvokeGet("Status") :?> SerivceStatus
            member x.Start() = entry.Invoke("Start") |> ignore
            member x.Stop() = entry.Invoke("Stop") |> ignore } :> obj)
        | _ -> None

    static member private Cast<'a> entry =
        IisAdmin.Materialize entry
        |> Option.bind (function
            | :? 'a as x -> Some(x)
            | _ -> None)
    
    static member AsWebSite entry = IisAdmin.Cast<IWebSite> entry
    static member AsVirtualDirectory entry = IisAdmin.Cast<IVirtualDirectory> entry     
    
    static member Open path =
        match IisAdmin.Materialize(new DirectoryEntry(path)) with
        | Some(x) -> x
        | None -> raise (ArgumentException "Invalid path")
        
    member x.FindVirtualDirectory name = 
        iis.PickChild (fun x -> 
            if System.String.Compare(x.Name, name, true) <> 0 then
                None
            else x |> IisAdmin.AsVirtualDirectory)
    
    member x.ListWebSites() = DirectoryEntry.children iis |> Seq.choose IisAdmin.AsWebSite
    
    member x.Item 
        with get name = iis.PickChild (fun x -> 
            if x.Name <> name then 
                None
            else IisAdmin.AsWebSite x)