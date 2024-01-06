module rec Budget.CLI.Program

open Argu
open Budget.Core
open Spectre.Console
open SpectreCoff

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
                | _ -> Some System.ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<Arguments>(programName = "budgetcli", errorHandler = errorHandler)

    let arguments = parser.ParseCommandLine argv

    let journalPath = arguments.GetResult(Journal)

    let readResult = IO.read journalPath

    match readResult with
    | Ok file -> parseJournal file
    | Error _ ->
        MarkupC(
            Color.Red,
            "There was an error reading the file. Check that it exists and that you have permission to read it."
        )
        |> toConsole

    0

let private parseJournal file =
    let result = file |> JournalParser.parse

    match result with
    | Ok journal -> $"{journal.Year}" |> Calm |> toConsole
    | Error JournalParser.InvalidSyntax ->
        // TODO: Improve error reporting.
        MarkupC(Color.Red, "There was an error parsing the file. Check that it is valid YAML.")
        |> toConsole
    | Error JournalParser.YearMissing ->
        Many
            [ Styles.error "You are missing a year field in your journal. Add it to the top of the file. For example:"
              Styles.error "year: 2021" ]
        |> toConsole
    | Error JournalParser.YearInvalid ->
        Many
            [ Styles.error "The year you specified is invalid. Use, you know, a normal year."
              Styles.hint
                  "(I mean, the app is checking for years before 1900 and after 3000, but you're not there, right?" ]
        |> toConsole
