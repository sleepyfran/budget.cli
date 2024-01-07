module Budget.Core.Tests.Utils

module Result =
    /// Returns the value from an Ok result, or throws an exception if the result is Error.
    let unwrap =
        function
        | Ok v -> v
        | Error _ -> failwith "Expected Ok, got Error"

    /// Returns the error from an Error result, or throws an exception if the result is Ok.
    let unwrapError =
        function
        | Ok _ -> failwith "Expected Error, got Ok"
        | Error e -> e
