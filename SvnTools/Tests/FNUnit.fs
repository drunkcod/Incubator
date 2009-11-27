namespace FNUnit
open NUnit.Framework

type FactAttribute = NUnit.Framework.TestAttribute

[<AutoOpen>]
module FNunitExtensions =
    let should what actual = what actual
    let be (expected:obj) actual = Assert.AreEqual(expected, actual)
    let contain wanted collection = Assert.Contains(wanted, collection)
