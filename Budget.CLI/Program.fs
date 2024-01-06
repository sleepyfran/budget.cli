open Argu
open Budget.Core
open Spectre.Console
open SpectreCoff
open System

type Arguments =
    | [<Mandatory; AltCommandLine("-j")>] Journal of path: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Journal _ -> "specify a journal file that contains a year budget"

[<EntryPoint>]
let main argv =
    let errorHandler =
        ProcessExiter(
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<Arguments>(programName = "budgetcli", errorHandler = errorHandler)

    let arguments = parser.ParseCommandLine argv

    let journalPath = arguments.GetResult(Journal)

    let readResult = IO.read journalPath

    match readResult with
    | Ok file -> Edgy file |> toConsole
    | Error _ ->
        MarkupC(
            Color.Red,
            "There was an error reading the file. Check that it exists and that you have permission to read it."
        )
        |> toConsole

    0
