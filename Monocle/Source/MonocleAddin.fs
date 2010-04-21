namespace Monocle
open NUnit.Core.Extensibility

[<NUnitAddin(Name = "Monocle")>]
type MonocleAddin() =
    interface IAddin with
        member this.Install host =
            let decorators = host.GetExtensionPoint("TestDecorators")
            if decorators = null then
                false
            else
                decorators.Install(TestNameFormatter())
                true