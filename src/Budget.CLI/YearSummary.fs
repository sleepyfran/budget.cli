module rec Budget.CLI.YearSummary

open Budget.Core
open Budget.Core.JournalSummary.Month
open Budget.Core.JournalSummary.Year
open Budget.Core.Model
open Spectre.Console
open SpectreCoff

let show journal =
    let summary = JournalSummary.Year.summarizeYear journal

    let containsEntries = not (summary.Months |> List.isEmpty)

    if containsEntries then
        show' summary
    else
        $"You don't have any entries in the journal." |> Styles.warn |> toConsole

let private show' summary =
    let allCategories = Utils.Union.allCasesOf<Category> ()

    let allMonthNames =
        summary.Months
        |> List.map (fun monthSummary -> Utils.Union.caseName monthSummary.Month)

    allCategories
    |> List.iter (fun category ->
        let categoryName = Utils.Union.caseName category

        let columns =
            [ categoryName; yield! allMonthNames; "Year" ] |> List.map Styles.tableColumn

        let rows =
            summary.CategoryTotals |> Map.find category |> toYearRows category summary

        let table =
            customTable
                { defaultTableLayout with
                    Border = TableBorder.Simple
                    Sizing = Expand }
                columns
                rows

        table.toOutputPayload |> toConsole)

    let totalsColumns =
        [ "Totals"; yield! allMonthNames; "Year" ] |> List.map Styles.tableColumn

    let totalsRows =
        [ Payloads
              [ Pumped "Expenses, goals and savings"
                yield! toMonthTotalRow summary (fun (monthSummary: MonthSummary) -> monthSummary.Totals.Total)
                Styles.money summary.YearTotals.Total ]
          Payloads
              [ Pumped "Total saved"
                yield! toMonthTotalRow summary (fun (monthSummary: MonthSummary) -> monthSummary.Totals.Saved)
                Styles.money summary.YearTotals.Saved ]
          Payloads
              [ Pumped "Left to spend"
                yield! toMonthTotalRow summary (fun (monthSummary: MonthSummary) -> monthSummary.Totals.LeftToSpend)
                Styles.money summary.YearTotals.LeftToSpend ] ]

    table totalsColumns totalsRows |> _.toOutputPayload |> toConsole

let private toYearRows category summary fieldTotals =
    fieldTotals
    |> Map.toList
    |> List.map (fun (fieldName, totalForYear) ->
        let monthValues =
            summary.Months
            |> List.map (fun summary ->
                summary.Entries
                |> Map.find category
                |> _.Fields
                |> List.tryFind (fun entry -> entry.Name = fieldName) (* TODO: Remove this filter once we transform this into a map. *)
                |> Option.map (fun entry -> Styles.money entry.Value)
                |> Option.defaultValue (Styles.money 0m))

        Payloads [ Vanilla fieldName; yield! monthValues; Styles.money totalForYear ])

let private toMonthTotalRow (summary: YearSummary) toRow =
    summary.Months |> List.map (toRow >> Styles.money)
