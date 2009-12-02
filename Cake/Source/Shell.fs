namespace Cake

open System.Diagnostics

module Shell =
    let Run commandLine = 
      let args = 
        ProcessStartInfo(
          FileName = "cmd",
          Arguments = "/C \"" + commandLine + "\"",
          UseShellExecute = false)
      let p = Process.Start(args)
      p.WaitForExit()
      if p.ExitCode <> 0 then
        failwith ("\"" + commandLine + "\" failed with exit code " + (string p.ExitCode))