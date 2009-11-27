namespace SvnTools
open System
open FNUnit

module SubversionLogEntryTests =
    let [<Fact>] Date_should_parse_RawDate() =
        let entry = SubversionLogEntry()
        entry.RawDate <- "2009-10-26T16:43:38.621867Z"
        entry.Date |> should be (DateTime.Parse("2009-10-26 16:43:38.621867"))