namespace Monocle
open System
open NUnit.Core.Extensibility

type TestNameFormatter() =    
    static member private RemoveSuffix suffix (s:string) =
        if s.EndsWith(suffix) then
            s.Substring(0, suffix.Length)
        else s

    static member Format (s:string) =
        s.Replace('_', ' ')
        |> TestNameFormatter.RemoveSuffix "Tests"
        
    interface ITestDecorator with
        member this.Decorate(test, memberInfo) =            
            test.TestName.Name <- TestNameFormatter.Format test.TestName.Name
            if not test.IsSuite then
                test.TestName.FullName <- test.TestName.Name
            test