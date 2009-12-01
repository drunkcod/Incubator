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
    let failingTarget = task <| fun () -> failwith "ZOMG! Failiure"
    let dependsOnFailure = [failingTarget] => task (fun () -> ())

module When_running_SampleBuild =
    [<SetUp>]
    let clear() = SampleBuild.targetsExecuted.Length <- 0
    let fake = FakeBuild(Assembly.GetExecutingAssembly())
    let invoke = fake.Invoke
    
    [<Test>]
    let should_run_each_dependency_only_once() =
        invoke "SampleBuild.d" |> ignore
        Assert.That(SampleBuild.targetsExecuted.ToString(), Is.EqualTo("abcd"))
    
    [<Test>]
    let should_raise_MissingTarget_for_missing_target() =
        let missingTargetRaised = ref false
        fake.MissingTarget <- fun x -> missingTargetRaised := true
        invoke "SampleBuild.missingTarget" |> ignore
        Assert.That(!missingTargetRaised, Is.True)
                
    [<Test>]
    let should_return_TargetMissing_for_missing_target() =
        Assert.That(invoke "SampleBuild.missingTarget", Is.EqualTo(Status.TargetMissing))   

    [<Test>]
    let should_return_TargetFailed_if_target_fails() =
        Assert.That(invoke "SampleBuild.failingTarget", Is.EqualTo(Status.TargetFailed))

    [<Test>]
    let should_return_TargetFail_if_dependency_fails() =
        Assert.That(invoke "SampleBuild.dependsOnFailure", Is.EqualTo(Status.TargetFailed))