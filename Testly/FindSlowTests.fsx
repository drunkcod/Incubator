open System
open System.Xml

let input = fsi.CommandLineArgs |> Seq.skip 1 |> Seq.hd
Console.WriteLine("Reading {0}.", input)
let xml = XmlReader.Create(input)

seq {
    while xml.Read() do
        if xml.NodeType = XmlNodeType.Element && xml.Name = "test-case" && xml.GetAttribute("executed") = "True" then
            yield (xml.GetAttribute("name"), xml.GetAttribute("time"))
}
|> Seq.map (fun (name, time) -> name, TimeSpan.FromSeconds(float time))
|> Seq.sortBy (fun x -> -(snd x))
|> Seq.take 25
|> Seq.iter (fun (name, time) -> Console.WriteLine("{0} - {1}", time, name))