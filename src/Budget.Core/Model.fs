module Budget.Core.Model

/// Defines a month.
type Month =
    | Leftover (* Obviously not a month, but we'll allow it so that transferring between years is not that awkward :D *)
    | January
    | February
    | March
    | April
    | May
    | June
    | July
    | August
    | September
    | October
    | November
    | December

/// Defines a type of entry in a month.
type Category =
    | Income
    | Expenses
    | Goals
    | Savings

/// Defines a field, with the name of the field and the value of the field.
type Field = { Name: string; Value: decimal }

/// Defines an entry in a month, with the name of the entry (for example, income) and the fields that it contains.
type MonthEntry =
    { Month: Month
      Entries: (Category * Field list) list }

/// Defines a journal, with the year that it defines and the entries for each month.
type Journal =
    { Year: int
      MonthEntries: MonthEntry list }

/// Defines the totals, which include the total out, total saved and total.
type Totals =
    { Out: decimal
      Saved: decimal
      Total: decimal
      LeftToSpend: decimal }
