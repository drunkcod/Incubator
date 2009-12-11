namespace SvnTools
open System
open System.IO
open System.Collections.Generic
open System.Xml.Serialization
open Xlnt.Stuff

module Xml =
    [<CompiledName("Read")>]
    let read<'a> (reader:TextReader)=
        let serializer = XmlSerializer(typeof<'a>)
        serializer.Deserialize(reader) :?> 'a

type SvnAction() =
    [<DefaultValue; XmlText>] 
    val mutable public Path : string
    [<DefaultValue; XmlAttribute("action")>] 
    val mutable public Action : string
    [<DefaultValue; XmlAttribute("kind")>]
    val mutable public Kind : string

    override this.GetHashCode() = hash this.Path
    override this.Equals(other) =
        match other with
        | :? SvnAction as other -> this.Path = this.Path && this.Action = this.Action && this.Kind = other.Kind
        | _ -> false

    override this.ToString() = String.Format("({0}:{1}({2})", this.Kind, this.Path, this.Action)

    static member File path action = SvnAction(Path = path, Action = action, Kind = "file")
    static member Directory path action = SvnAction(Path = path, Action = action, Kind = "dir")

[<XmlRoot("logentry")>]
type SubversionLogEntry() =
    [<DefaultValue; XmlAttribute("revision")>] 
    val mutable public Revision : int
    [<DefaultValue; XmlElement("author")>]
    val mutable public Author : string
    [<DefaultValue; XmlElement("msg")>]
    val mutable public  Message : string
    [<DefaultValue; XmlElement("date")>]
    val mutable public RawDate : string

    let actions = List<SvnAction>()

    [<XmlArray("paths");XmlArrayItem("path")>]
    member this.Actions = actions;

    member this.Date = DateTime.Parse(this.RawDate).ToUniversalTime()

[<XmlRoot("log")>]
type SubversionLog() =
    let entries = List<SubversionLogEntry>()

    [<XmlElement("logentry")>]
    member this.Entries = entries

type Row = {
    Date : string
    Added : int
    Deleted : int }
    
module Program =
        
    let findOrDefault m def key = 
        Map.tryFind key m
        |> Option.getOrDefault def

    let byDate f (entries:SubversionLogEntry seq) = 
        entries
        |> Seq.groupBy (fun x -> x.Date.ToString("yyyy-MM-dd"))
        |> Seq.map (fun (date, entries) -> date, f entries)
            
    let countActions (entries:SubversionLogEntry seq) =            
        entries |> Seq.collect (fun x -> x.Actions)       
        |> Seq.filter (fun x -> x.Path.EndsWith(".cs") || x.Path.EndsWith(".rb") || x.Path.EndsWith(".vb"))
        |> Seq.countBy  (fun x -> x.Action)

    let countAuthor = Seq.countBy (fun (x:SubversionLogEntry) -> x.Author)        

    let join sep s = s |> Seq.reduce (fun x y -> x + sep + y)

    let [<EntryPoint>] main team =
        let log = Xml.read<SubversionLog> Console.In
        let data = log.Entries |> byDate countAuthor

        Console.WriteLine("{0}; {1}", "date", team |> join "; ")
        data |> Seq.sortBy fst
        |> Seq.iter (fun (date, actions) ->
            let get = findOrDefault (Map.ofSeq actions) 0    
            let data = team |> Seq.mapi (fun n who -> string (n + 1) + "; " + (get >> string) who) |> join "; "
            Console.WriteLine("{0}; {1}", date, data))
        0