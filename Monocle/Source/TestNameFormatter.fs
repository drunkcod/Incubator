namespace Monocle
open System
open NUnit.Core.Extensibility

type TestNameFormatter() =    

    member this.Format (s:string) =        
        let TestsSuffix = "Tests"

        let s' = s.Replace('_', ' ')
        if s'.EndsWith TestsSuffix  then
            s.Substring(0, s.Length - TestsSuffix.Length)
        else s'

    interface ITestDecorator with
        member this.Decorate(test, memberInfo) =            
            test.TestName.Name <- this.Format test.TestName.Name
            if not test.IsSuite then
                test.TestName.FullName <- test.TestName.Name
            test