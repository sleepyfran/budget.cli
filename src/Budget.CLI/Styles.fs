module Budget.CLI.Styles

open Spectre.Console
open SpectreCoff

let error str = MarkupC(Color.Red, str)

let warn str = MarkupC(Color.Yellow, str)

let hint str = MarkupC(Color.Grey, str)

let private decimal (value: decimal<_>) =
    System.String.Format("{0:#,##0.00}", value)

let money amount =
    let formatted = decimal amount

    if amount < 0m then MarkupC(Color.Red, formatted)
    else if amount = 0m then MarkupC(Color.Grey, formatted)
    else MarkupC(Color.Green, formatted)

let tableColumnColor = Calm

let tableColumn str =
    tableColumnColor str
    |> column
    |> withLayout
        { defaultColumnLayout with
            Alignment = Left }
