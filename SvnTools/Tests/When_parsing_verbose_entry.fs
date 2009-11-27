namespace SvnTools
open System.Collections.Generic
open System.IO
open FNUnit

module When_parsing_verbose_sample_entry =
    let entry =
        using(File.OpenText("TestData/VerboseEntry.xml")) Xml.read<SubversionLogEntry>

    let [<Fact>] it_contains_a_modified_file() =
        let item = SvnAction.File "/ModifiedFile" "M"
        entry.Actions |> should contain item

    let [<Fact>] it_contains_a_modified_directory() =
        let item = SvnAction.Directory "/ModifiedDirectory" "M"
        entry.Actions |> should contain item

    let [<Fact>] it_contains_a_added_file() =
        let item = SvnAction.File "/AddedFile" "A"
        entry.Actions |> should contain item

    let [<Fact>] it_contains_a_deleted_file() =
        let item = SvnAction.File "/DeletedFile" "D"
        entry.Actions |> should contain item
