open System
open System.Collections
open System.Collections.Generic
open System.Xml

[<Sealed>]
type private TakeMaxEnumerator<'a>(items:IEnumerator<'a>, count) =
    let mutable n = 0
    interface IEnumerator<'a> with
        member this.Current = items.Current
    interface IEnumerator with
        member this.MoveNext() =
            if n = count then
                false
            else
                n <- n + 1
                items.MoveNext()
        member this.Reset() = items.Reset()
        member this.Current = items.Current :> obj
    interface IDisposable with
        member this.Dispose() = items.Dispose()                                                                

module Seq =
    let makeSeq x = {
        new IEnumerable<'a> with
            member this.GetEnumerator() = x() :> 'a IEnumerator 
        interface IEnumerable with
            member this.GetEnumerator() = x() :> IEnumerator}   

    let takeMax (count:int) (x:'a seq) = makeSeq(fun () -> new TakeMaxEnumerator<'a>(x.GetEnumerator(), count))
            
let input = fsi.CommandLineArgs |> Seq.skip 1 |> Seq.hd
Console.WriteLine("Reading {0}.", input)
let xml = XmlReader.Create(input)

seq { while xml.Read() do
        if xml.NodeType = XmlNodeType.Element && xml.Name = "test-case" && Convert.ToBoolean(xml.GetAttribute("executed")) then
            yield (xml.GetAttribute("name"), xml.GetAttribute("time"))}
|> Seq.map (fun (name, time) -> name, TimeSpan.FromSeconds(float time))
|> Seq.sortBy (fun x -> -(snd x))
|> Seq.takeMax 25
|> Seq.iter (fun (name, time) -> Console.WriteLine("{0} - {1}", time, name))