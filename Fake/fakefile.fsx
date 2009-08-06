open System
open System.Diagnostics

let exec commandLine = 
  let args = 
    ProcessStartInfo(
      Arguments = "/C \"" + commandLine + "\"",
      FileName = "cmd",
      UseShellExecute = false)
  let p = Process.Start(args)
  p.WaitForExit()
  if p.ExitCode <> 0 then
    failwith ("\"" + commandLine + "\" failed with exit code " + (string p.ExitCode))

let args = fsi.CommandLineArgs |> Seq.skip 1

let steps = [|
  "svn up trunk";
  "cd trunk && rake -n build:release build:test_cint";
  "svn up BuildEnvironment";
  "svn up BuildEnvironment\\trunk";
  "cd BuildEnvironment && go clean compile"|]

steps |> Seq.iter exec
