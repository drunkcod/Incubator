namespace Xlnt.Stuff
open System
open System.Collections.Generic
open System.DirectoryServices

type WebSiteStatus = 
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
    abstract Status : WebSiteStatus
    abstract Start : unit -> unit
    abstract Stop : unit -> unit

and IVirtualDirectory =
    abstract Name : string
    abstract WebSite : IWebSite       

type private DirectoryEntryVirtualDirectory(entry:DirectoryEntry) =
    interface IVirtualDirectory with
        member x.Name = entry.Name
        member x.WebSite = entry.PickParent IisAdmin.AsWebSite
    override x.ToString() = entry.Path        

and IisAdmin(path) = 
    let iis = new DirectoryEntry(path)

    static member AsVirtualDirectory (entry:DirectoryEntry) = 
        if entry.SchemaClassName <> "IIsWebVirtualDir" then 
            None
        else Some(DirectoryEntryVirtualDirectory(entry) :> IVirtualDirectory)
            
    static member AsWebSite (entry:DirectoryEntry) =
        if entry.SchemaClassName <> "IIsWebServer" then
            None
        else Some({ new IWebSite with
            member x.Name = entry.Name
            member x.Children = entry.Children
            member x.VirtualDirectories =
                DirectoryEntry.flatten entry
                |> Seq.choose IisAdmin.AsVirtualDirectory
            member x.Properties = entry.Properties
            member x.Status = entry.InvokeGet("Status") :?> WebSiteStatus
            member x.Start() = entry.Invoke("Start") |> ignore
            member x.Stop() = entry.Invoke("Stop") |> ignore })        

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