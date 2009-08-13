namespace Fake
open Fake
open System.Text
open System.Reflection
open NUnit.Framework

(*
    Sample diamond shaped build.
       B
    D < > A
       C
*)
module SampleBuild =
    let targetsExecuted = StringBuilder()
    let log (value:string) = task <| fun () -> targetsExecuted.Append(value) |> ignore
    let a = log "a"
    let b = [a] => log "b"
    let c = [a] => log "c"
    let d = [b; c] => log "d"

module When_running_SampleBuild_target_d =
    [<SetUp>]
    let clear() = SampleBuild.targetsExecuted.Length <- 0
    
    [<Test>]
    let should_run_each_dependency_only_once() =
        invoke (Assembly.GetExecutingAssembly()) "SampleBuild.d"
        Assert.That(SampleBuild.targetsExecuted.ToString(), Is.EqualTo("abcd"))