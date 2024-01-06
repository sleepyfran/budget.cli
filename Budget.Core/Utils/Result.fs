module Budget.Core.Utils.Result

/// Creates a result that returns Ok if the option is Some, or a specific error if the option is None.
let fromOption (err: 'error) (opt: 'a option) : Result<'a, 'error> =
    match opt with
    | Some x -> Ok x
    | None -> Error err
