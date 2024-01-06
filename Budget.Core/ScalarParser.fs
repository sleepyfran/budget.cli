module Budget.Core.ScalarParser

module Int =
    /// Attempts to parse a string into an int.
    let fromStr (str: string) =
        match System.Int32.TryParse str with
        | true, int -> Some int
        | _ -> None
