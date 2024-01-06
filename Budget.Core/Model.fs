module Budget.Core.Model

/// Defines a month.
type Month =
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
type MonthEntryType =
    | Income
    | Expenses
    | Goals
    | Savings

/// Defines a field, with the name of the field and the value of the field.
type Field = { Name: string; Value: string }

/// Defines an entry in a month, with the name of the entry (for example, income) and the fields that it contains.
type MonthEntry =
    { Type: MonthEntryType
      Fields: Field list }

/// Defines a journal, with the year that it defines and the entries for each month.
type Journal =
    { Year: int
      MonthEntries: Map<Month, MonthEntry list> }
