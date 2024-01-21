module rec Budget.Core.JournalSummary.Month

open Budget.Core.Model

let private incomeCategories = [ Income ]
let private expenseCategories = [ Expenses ]
let private savingsCategories = [ Goals; Savings ]

/// Defines an entry of a month with the summary details, which include the total out, total saved and total.
type CategoryWithTotals = { Fields: Field list; Total: decimal }

/// Defines the summary of a month, which includes the month and the entries with the totals.
type MonthSummary =
    { Month: Month
      Entries: Map<Category, CategoryWithTotals>
      Totals: Totals }

/// Summarizes the given month of a journal, which includes all the amounts as they were written on the journal for
/// this month, as well as the totals for each category and a in/out total for the month.
let summarizeMonth month (journal: Journal) =
    let entriesByCategoryWithTotals =
        journal.MonthEntries
        |> List.tryFind (fun entry -> entry.Month = month)
        |> Option.map (fun monthEntry ->
            monthEntry.Entries
            |> Map.ofList
            |> Map.map (fun _ fields ->
                let total = fields |> List.sumBy _.Value

                { Fields = fields; Total = total }))
        |> Option.defaultValue Map.empty

    let incomeTotal = sumCategoriesTotals incomeCategories entriesByCategoryWithTotals
    let expenseTotal = sumCategoriesTotals expenseCategories entriesByCategoryWithTotals
    let savingsTotal = sumCategoriesTotals savingsCategories entriesByCategoryWithTotals

    let totalSpent = expenseTotal + savingsTotal

    let totals =
        { Out = expenseTotal
          Saved = savingsTotal
          Total = totalSpent
          LeftToSpend = incomeTotal - totalSpent }

    { Month = month
      Entries = entriesByCategoryWithTotals
      Totals = totals }

let private sumCategoriesTotals categories entriesByCategoryWithTotals =
    categories
    |> List.map (fun category ->
        let categoryWithTotals = Map.tryFind category entriesByCategoryWithTotals

        match categoryWithTotals with
        | Some categoryWithTotals -> categoryWithTotals.Total
        | None -> 0m)
    |> List.sumBy id
