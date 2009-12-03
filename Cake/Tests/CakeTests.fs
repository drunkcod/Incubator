namespace Cake
open Cake
open System.Text
open System.Reflection
open FNUnit

(*
    Sample diamond shaped build.
       B
    D < > A
       C
*)
module SampleBuild =
    let ExecutedTargets = StringBuilder()
    let log (value:string) = task <| fun () -> ExecutedTargets.Append(value) |> ignore
    let a = log "a"
    let b = [a] => log "b"
    let c = [a] => log "c"
    let d = [b; c] => log "d"
    let failingTarget = task <| fun () -> failwith "ZOMG! Failiure"
    let dependsOnFailure = [failingTarget] => task (fun () -> ())

module When_running_SampleBuild =
    let ExecutedTargets() = SampleBuild.ExecutedTargets.ToString()
    
    [<Setup>]
    let clear() = SampleBuild.ExecutedTargets.Length <- 0
    let Cake = CakeBuild(Assembly.GetExecutingAssembly())
    let invoke = Cake.Invoke

    [<Fact>]
    let should_run_each_dependency_only_once() =
        invoke "SampleBuild.d" |> ignore
        ExecutedTargets() |> should be "abcd"

    [<Fact>]
    let should_raise_MissingTarget_for_missing_target() =
        let missingTargetRaised = ref false
        Cake.MissingTarget <- fun x -> missingTargetRaised := true
        invoke "SampleBuild.missingTarget" |> ignore
        !missingTargetRaised |> should be true

    [<Fact>]
    let should_return_TargetMissing_for_missing_target() =
        invoke "SampleBuild.missingTarget" |> should be Status.TargetMissing

    [<Fact>]
    let should_return_TargetFailed_if_target_fails() =        
        invoke "SampleBuild.failingTarget" |> should be Status.TargetFailed

    [<Fact>]
    let should_return_TargetFail_if_dependency_fails() =        
        invoke "SampleBuild.dependsOnFailure" |> should be Status.TargetFailed