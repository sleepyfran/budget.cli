module rec Budget.CLI.Program

open Argu
open Budget.Core
open Spectre.Console
open SpectreCoff

type Arguments =
    | [<MainCommand; ExactlyOnce>] Journal of path: string
    | [<AltCommandLine("-m")>] Month of month: Model.Month
    | [<AltCommandLine("-y")>] Year
    | Sample

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Journal _ -> "journal file that contains a year budget"
            | Month _ ->
                "specify a specific month to show (e.g. \"january\"). If not specified, the current month is used"
            | Year -> "shows the year summary"
            | Sample -> "loads an internal test journal instead of reading it from the path"

type private SelectedOption =
    | Year
    | Month of Model.Month

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

    let selectedOption =
        let monthOpt = arguments.TryGetResult(Arguments.Month)
        let yearOpt = arguments.TryGetResult(Arguments.Year)

        match yearOpt, monthOpt with
        | Some _, _ -> Year
        | None, Some month -> Month month
        | None, None -> Utils.Calendar.currentMonth () |> Month

    let readResult = readFile arguments

    match readResult with
    | Ok fileContent -> JournalParser.parse fileContent |> showSelectedOption selectedOption
    | Error _ ->
        MarkupC(
            Color.Red,
            "There was an error reading the file. Check that it exists and that you have permission to read it."
        )
        |> toConsole

    0

let private readFile (arguments: ParseResults<Arguments>) =
    let journalPath = arguments.GetResult(Journal)
    let sampleMode = arguments.Contains(Sample)

    if sampleMode then
        System.IO.Directory.GetParent(__SOURCE_DIRECTORY__)
        |> fun baseDir -> System.IO.Path.Combine(baseDir.FullName, "Budget.CLI/Resources/SampleJournal.yml")
        |> IO.read
    else
        IO.read journalPath

let private showSelectedOption opt journalResult =
    match journalResult with
    | Ok journal ->
        match opt with
        | Year -> YearSummary.show journal
        | Month month -> MonthSummary.show month journal
    | Error err -> Errors.fromParsing err |> toConsole
