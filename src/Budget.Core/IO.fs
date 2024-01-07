module rec Budget.Core.IO

open System
open System.IO

/// Defines what went wrong when reading a file.
type IOError =
    | FailedToReadFile
    | Unknown

/// Reads all lines from a file into a string.
let read path =
    try
        path |> expandPath |> File.ReadAllText |> Ok
    with
    | :? DirectoryNotFoundException
    | :? IOException
    | :? UnauthorizedAccessException -> Error FailedToReadFile
    | _ -> Error Unknown

// Expands a tilde in the beginning of a path to the user's home directory.
let private expandPath (path: string) =
    let needsExpansion = path.StartsWith("~")

    if needsExpansion then
        path
            .Remove(0)
            .Insert(0, Environment.GetFolderPath Environment.SpecialFolder.UserProfile)
    else
        path
