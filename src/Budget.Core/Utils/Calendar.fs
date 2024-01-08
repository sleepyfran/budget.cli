module Budget.Core.Utils.Calendar

open Budget.Core.Model
open System

/// Returns the current month.
let currentMonth () =
    let allMonths = Union.allCasesOf<Month> ()
    let currentMonthIndex = DateTime.Now.Month
    allMonths[currentMonthIndex - 1]

/// Attempts to parse a month from a string.
let monthByName name =
    let allMonths = Union.allCasesOf<Month> ()

    allMonths
    |> List.tryFind (fun m -> (m.ToString() |> String.lowercased) = (name |> String.lowercased))
