namespace Monocle
open System
open NUnit.Core.Extensibility

type TestNameFormatter() =    

    static member format (s:string) =        
        let TestsSuffix = "Tests"

        let s' = s.Replace('_', ' ')

        if s'.EndsWith TestsSuffix  then
            s.Substring(0, TestsSuffix .Length - s.Length)
        else s'

    interface ITestDecorator with
        member this.Decorate(test, memberInfo) =            
            test.TestName.Name <- TestNameFormatter.format test.TestName.Name
            if not test.IsSuite then
                test.TestName.FullName <- test.TestName.Name
            test