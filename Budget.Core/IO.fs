module Budget.Core.IO

open System
open System.IO

/// Defines what went wrong when reading a file.
type IOError =
    | FailedToReadFile
    | Unknown

/// Reads all lines from a file into a string.
let read path =
    try
        File.ReadAllText path |> Ok
    with
    | :? DirectoryNotFoundException
    | :? IOException
    | :? UnauthorizedAccessException -> Error FailedToReadFile
    | _ -> Error Unknown
