namespace SvnTools
open System.Collections.Generic
open System.IO
open FNUnit

module When_parsing_sample_log =
    let log =
        using(File.OpenText("TestData/SampleLog.xml")) Xml.read<SubversionLog>

    let [<Fact>] it_should_have_10_entries() =
        log.Entries.Count |> should be 10