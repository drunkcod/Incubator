namespace Fake

open System.Diagnostics

module Shell =
    let run commandLine = 
      let args = 
        ProcessStartInfo(
          Arguments = "/C \"" + commandLine + "\"",
          FileName = "cmd",
          UseShellExecute = false)
      let p = Process.Start(args)
      p.WaitForExit()
      if p.ExitCode <> 0 then
        failwith ("\"" + commandLine + "\" failed with exit code " + (string p.ExitCode))