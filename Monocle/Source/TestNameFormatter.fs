namespace Monocle
open System
open NUnit.Core.Extensibility

type TestNameFormatter() =
    static member private RemoveSuffix suffix (s:string) =
        if s.EndsWith(suffix) then
            s.Substring(0, suffix.Length)
        else s

    member this.Format (s:string) =
        s.Replace('_', ' ')
        |> TestNameFormatter.RemoveSuffix "Tests"

    interface ITestDecorator with
        member this.Decorate(test, memberInfo) =
            test.TestName.Name <- this.Format test.TestName.Name
            test