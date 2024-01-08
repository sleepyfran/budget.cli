module rec Budget.CLI.MonthSummary

open Budget.Core
open Budget.Core.Model
open Spectre.Console
open SpectreCoff

let show month journal =
    let summary = JournalSummary.Month.summarizeMonth month journal

    let containsEntries = not (summary.Entries |> Map.isEmpty)

    if containsEntries then
        show' summary
    else
        $"You don't have any entries for \"{month}\"." |> Styles.warn |> toConsole

let private show' summary =
    let categoryTables =
        summary.Entries
        |> Map.map (fun category fieldsWithTotal ->
            let categoryName = Utils.Union.caseName category

            let fieldsColumns =
                [ Styles.tableColumn "Field"
                  Styles.tableColumn "Amount" |> withFooter (Styles.money fieldsWithTotal.Total) ]

            let fieldsRows =
                fieldsWithTotal.Fields
                |> List.map (fun item -> Payloads [ Vanilla item.Name; Styles.money item.Value ])

            let fieldsTable =
                customTable
                    { defaultTableLayout with
                        Border = TableBorder.Simple
                        Sizing = Collapse }
                    fieldsColumns
                    fieldsRows
                |> withTitle categoryName

            fieldsTable.toOutputPayload)
        |> List.ofSeq
        |> List.map (_.Value)

    categoryTables
    |> List.chunkBySize 2
    |> List.map Payloads
    |> grid
    |> _.toOutputPayload
    |> toConsole

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
