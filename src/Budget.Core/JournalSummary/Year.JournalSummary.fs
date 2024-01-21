module rec Budget.Core.JournalSummary.Year

open Budget.Core.JournalSummary.Month
open Budget.Core.Model
open Budget.Core.Utils

type YearSummary =
    { Months: MonthSummary list
      YearTotals: Totals
      CategoryTotals: Map<Category, Map<string, decimal>> }

/// Summarizes a year of a journal, which returns the collection of all present months and the totals for the entire
/// year for each category found in the journal.
let summarizeYear (journal: Journal) =
    let allMonthSummaries =
        Union.allCasesOf<Month> ()
        |> List.map (fun month -> Month.summarizeMonth month journal)
        |> List.filter (fun monthSummary -> not (monthSummary.Entries |> Map.isEmpty))

    let allCategories = Union.allCasesOf<Category> ()

    let categoryTotals =
        (allMonthSummaries, allCategories)
        ||> List.allPairs
        |> List.fold
            (fun (acc: Map<Category, Map<string, decimal>>) (summary, category) ->
                let categoryWithTotals =
                    summary.Entries
                    |> Map.find category (* We're guaranteed that the journal is invalid without a category. *)

                let overallTotals = acc |> Map.tryFind category |> Option.defaultValue Map.empty

                let updatedCategoryOverallTotals =
                    categoryWithTotals.Fields
                    |> List.fold
                        (fun totals field ->
                            totals
                            |> Map.change field.Name (fun value ->
                                match value with
                                | Some value -> Some(value + field.Value)
                                | None -> Some field.Value))
                        overallTotals

                acc |> Map.add category updatedCategoryOverallTotals)
            Map.empty

    let yearTotals: Totals =
        { Out = allMonthSummaries |> List.sumBy _.Totals.Out
          Saved = allMonthSummaries |> List.sumBy _.Totals.Saved
          Total = allMonthSummaries |> List.sumBy _.Totals.Total
          LeftToSpend = allMonthSummaries |> List.sumBy _.Totals.LeftToSpend }

    { Months = allMonthSummaries
      CategoryTotals = categoryTotals
      YearTotals = yearTotals }
