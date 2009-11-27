namespace SvnTools
open System.Collections.Generic
open System.IO
open FNUnit

module When_parsing_sample_entry =
    let entry =
        using(File.OpenText("TestData/SampleEntry.xml")) Xml.read<SubversionLogEntry>

    let [<Fact>] it_should_have_revision_1234() =
        entry.Revision |> should be 1234

    let [<Fact>] it_should_have_author_drunkcod() =
        entry.Author |> should be "drunkcod"

    let [<Fact>] it_should_have_a_proper_message() =
        entry.Message |> should be "Did something funky"

    let [<Fact>] is_should_contain_the_proper_date() =
        entry.RawDate |> should be "2009-10-26T16:43:38.621867Z"