[<AutoOpen>]
module Moncole.Moq
open System
open System.Linq.Expressions
open Microsoft.FSharp.Quotations
open Moq

let private value = function
    | Patterns.Value(obj, t) -> Expression.Constant(obj) :> Expression
    | _ -> raise (InvalidOperationException())

type MockAction<'a,'b when 'a : not struct> = 
    | PropertyGet of Expression<Func<'a, 'b>>
    | Call of Expression<Func<'a, 'b>>

type MockSetup<'a,'b when 'a : not struct> =
    | PropertySetup of Moq.Language.Flow.ISetupGetter<'a, 'b>
    | CallSetup of Moq.Language.Flow.ISetup<'a, 'b>

let private lambda(body, input) = Expression.Lambda<Func<'a, 'b>>(body, [|input|])

let private toAction = function
    | Patterns.Lambda(var, e) -> 
        let input = Expression.Parameter(var.Type, var.Name)
        match e with
        | Patterns.PropertyGet(this, property, args) ->
            let body = Expression.Property(input, property)
            PropertyGet(lambda(body, input))

        | Patterns.Call(this, targetMethod, args) ->
            let args = args |> Seq.map value |> Seq.toArray
            let body = Expression.Call(input, targetMethod, args)
            Call(lambda(body, input))

        | _ -> raise (InvalidOperationException())
    | _ -> raise (InvalidOperationException())

let private toSetup (mock:'a Mock) = function
        | PropertyGet(func) -> PropertySetup(mock.SetupGet(func))
        | Call(func) -> CallSetup(mock.Setup(func))

let doing(expr:Expr<'a->'b>) setupResult (mock:'a Mock) = 
    expr |> (toAction >> toSetup mock)
    |> setupResult
    |> ignore
    mock
    
let returns (value:'b) = function
    | PropertySetup(x) -> x.Returns(value)
    | CallSetup(x) -> x.Returns(value)