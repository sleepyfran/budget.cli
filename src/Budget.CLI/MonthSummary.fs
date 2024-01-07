module Budget.CLI.MonthSummary

open Budget.Core
open Budget.Core.Model
open Spectre.Console
open SpectreCoff
open System
open SpectreCoff.Styling

let show journal =
    let allMonths = Utils.Union.allCasesOf<Model.Month> ()
    let currentMonthIndex = DateTime.Now.Month
    let currentMonth = allMonths[currentMonthIndex - 1]

    let summary = JournalSummary.Month.summarizeMonth currentMonth journal

    let categoryTables =
        summary.Entries
        |> Map.map (fun category fieldsWithTotal ->
            let categoryName = Utils.Union.caseName category

            let fieldsColumns = [ Styles.tableColumn "Field"; Styles.tableColumn "Amount" ]

            let fieldsRows =
                fieldsWithTotal.Fields
                |> List.map (fun item -> Payloads [ Vanilla item.Name; Styles.money item.Value ])

            let fieldsTable =
                customTable
                    { defaultTableLayout with
                        Border = TableBorder.Minimal
                        Sizing = Collapse }
                    fieldsColumns
                    fieldsRows

            Many [ Calm categoryName; fieldsTable.toOutputPayload ])
        |> List.ofSeq
        |> List.map (_.Value)

    grid [ Payloads categoryTables ] |> _.toOutputPayload |> toConsole

    let totalsColumns = [ Styles.tableColumn "Name"; Styles.tableColumn "Amount" ]

    let totalsRows =
        [ Payloads [ Pumped "Expenses, goals and savings"; Styles.money summary.Totals.Total ]
          Payloads [ Pumped "Total saved"; Styles.money summary.Totals.Saved ]
          Payloads [ Pumped "Left to spend"; Styles.money summary.Totals.LeftToSpend ] ]

    let totalsTable =
        customTable
            { defaultTableLayout with
                HideHeaders = true }
            totalsColumns
            totalsRows

    Many [ Calm "Totals"; totalsTable.toOutputPayload ] |> toConsole
