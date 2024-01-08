module rec Budget.CLI.Program

open Argu
open Budget.Core
open Spectre.Console
open SpectreCoff

type Arguments =
    | [<MainCommand; ExactlyOnce>] Journal of path: string
    | [<AltCommandLine("-m")>] Month of month: Model.Month

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Journal _ -> "journal file that contains a year budget"
            | Month _ ->
                "specify a specific month to show (e.g. \"january\"). If not specified, the current month is used"

[<EntryPoint>]
let main argv =
    let errorHandler =
        ProcessExiter(
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some System.ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<Arguments>(programName = "budgetcli", errorHandler = errorHandler)

    let arguments = parser.ParseCommandLine argv

    let journalPath = arguments.GetResult(Journal)

    let month =
        arguments.TryGetResult(Month) |> Option.defaultWith Utils.Calendar.currentMonth

    let readResult = IO.read journalPath

    match readResult with
    | Ok file -> parseJournal month file
    | Error _ ->
        MarkupC(
            Color.Red,
            "There was an error reading the file. Check that it exists and that you have permission to read it."
        )
        |> toConsole

    0

let private parseJournal month file =
    let result = file |> JournalParser.parse

    match result with
    | Ok journal -> MonthSummary.show month journal
    | Error err -> Errors.fromParsing err |> toConsole
