module Budget.CLI.Styles

open Spectre.Console
open SpectreCoff

let error str = MarkupC(Color.Red, str)

let hint str = MarkupC(Color.DarkSlateGray1, str)
