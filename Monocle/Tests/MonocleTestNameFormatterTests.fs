[<NUnit.Framework.TestFixture>]
module Monocle.TestNameFormatterTests
open Moncole
open System
open NUnit.Framework
open NUnit.Core
open NUnit.Core.Extensibility
open Moq

let inline should be expected actual = be expected actual
let inline be (expected:obj) (actual:obj) =
    Assert.That(actual, Is.EqualTo(expected))

let formatter = TestNameFormatter()
let decorate x = (formatter :> ITestDecorator).Decorate(x, null)

[<Test>]
let should_convert_underscore_to_space() =
    formatter.Format "should_convert_underscore_to_space"
    |> should be "should convert underscore to space"

[<Test>]
let should_convert_plus_to_space() =
    formatter.Format "should+convert+plus+to+space"
    |> should be "should convert plus to space"

[<Test>]
let it_removes_traling_Tests_from_name() =
    formatter.Format "TestNameFormatterTests"
    |> should be "TestNameFormatter"

[<Test>]
let wont_tamper_with_full_name_for_suite() =
    let test =
        Mock<Test>(TestName(Name = "Name", FullName = "FullName"))
        |> doing <@ fun x -> x.IsSuite @> (returns true)

    decorate test.Object
    |> (fun x -> x.TestName.FullName)
    |> should be "FullName"