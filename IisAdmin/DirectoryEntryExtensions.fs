[<AutoOpen>]
module Xlnt.Stuff.DirectoryEntry 
open System.DirectoryServices

let children (x:DirectoryEntry) = x.Children |> Seq.cast<DirectoryEntry>
let rec flatten (x:DirectoryEntry) = seq { 
    yield x 
    yield! x.Children |> Seq.cast<DirectoryEntry> |> Seq.collect flatten }

type System.DirectoryServices.DirectoryEntry with   
    member x.PickChild p = flatten x |> Seq.pick p
    
    member x.PickParent (p:DirectoryEntry->'a option) =
        let rec loop node = 
            match p(node) with
            | Some(x) -> x
            | None -> loop node.Parent
        loop x.Parent
        