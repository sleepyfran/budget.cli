module rec Budget.CLI.Program

open Argu
open Budget.Core
open Spectre.Console
open SpectreCoff

type Arguments =
    | [<Mandatory; AltCommandLine("-j")>] Journal of path: string
    | [<AltCommandLine("-m")>] Month of month: Model.Month

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Journal _ -> "specify a journal file that contains a year budget"
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
    | Error JournalParser.InvalidSyntax ->
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
    | Error(JournalParser.InvalidMonth monthName) ->
        Styles.error
            $"The month \"{monthName}\" is not a valid month. Make sure you have a valid month name in your journal."
        |> toConsole
    | Error(JournalParser.MissingEntry(monthName, entryName)) ->
        Styles.error $"The entry \"{entryName}\" is missing from the month \"{monthName}\"."
        |> toConsole
    | Error(JournalParser.InvalidEntry(monthName, entryName)) ->
        Styles.error
            $"The entry \"{entryName}\" is invalid in the month \"{monthName}\". Make sure you have a valid entry name in your journal."
        |> toConsole
    | Error(JournalParser.InvalidField(monthName, entryName)) ->
        Many
            [ Styles.error $"A field inside the entry \"{entryName}\" of the month {monthName} is invalid."
              Styles.hint "Make sure you have a all fields inside an entry have the format \"- name: value\"" ]
        |> toConsole
    | Error(JournalParser.InvalidFieldValue(monthName, entryName, fieldName)) ->
        Styles.error
            $"The field \"{fieldName}\", inside the entry \"{entryName}\" of the month \"{monthName}\" contains an invalid value. Make sure the value is a decimal number."
        |> toConsole
