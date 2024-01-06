module Budget.Core.ScalarParser

module Decimal =
    /// Attempts to parse a string into a decimal.
    let fromStr (str: string) =
        match System.Decimal.TryParse(str) with
        | true, decimal -> Some decimal
        | _ -> None

module Int =
    /// Attempts to parse a string into an int.
    let fromStr (str: string) =
        match System.Int32.TryParse str with
        | true, int -> Some int
        | _ -> None
