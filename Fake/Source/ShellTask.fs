namespace Fake

type ShellTask() =
    [<DefaultValue>] val mutable Program : string
    [<DefaultValue>] val mutable Arguments : string

    interface ITarget with
        member this.Run() = Shell.Run(this.Program + " " + this.Arguments)