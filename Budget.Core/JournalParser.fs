module rec Budget.Core.JournalParser

open Budget.Core.Model
open Budget.Core.Utils
open FsToolkit.ErrorHandling
open System.IO
open YamlDotNet.RepresentationModel

type ParsingError =
    | InvalidSyntax
    | YearMissing
    | YearInvalid
    | InvalidMonth of monthName: string
    | MissingEntry of monthName: string * entryName: string
    | InvalidEntry of monthName: string * entryName: string
    | InvalidField of monthName: string * entryName: string
    | InvalidFieldValue of monthName: string * entryName: string * fieldName: string

type private NodeChildren = Map<string, YamlNode>

/// <summary>
/// Attempts to parse the given string as a journal. Returns a result with either the parsed journal model or an error
/// with the reason for the failure.
/// </summary>
/// <param name="contents">Content of the journal as read from a file or some other medium.</param>
let parse (contents: string) =
    try
        parse' contents
    with exn ->
        exn |> System.Console.WriteLine
        Error InvalidSyntax

let private parse' (contents: string) =
    use contentReader = new StringReader(contents)
    let yamlStream = YamlStream()
    yamlStream.Load(contentReader)

    // The root should always be a mapping node.
    let root = yamlStream.Documents[0].RootNode

    match root with
    | Mapping(_, children) ->
        result {
            let! year = tryParseYear children
            let! months = tryParseMonths children

            return { Year = year; MonthEntries = months }
        }
    | _ -> Error InvalidSyntax

let private tryParseYear (root: NodeChildren) =
    let yearNode = root |> Map.tryFind "year"

    match yearNode with
    | Some(Scalar(_, year)) ->
        result {
            let! year = ScalarParser.Int.fromStr year |> Result.fromOption YearInvalid

            if year < 1900 || year > 3000 then
                return! Error YearInvalid
            else
                return year
        }
    | Some _ -> Error YearInvalid
    | None -> Error YearMissing

let private tryParseMonths (root: NodeChildren) =
    Union.allCasesOf<Month> ()
    |> List.choose (tryParseMonth root) (* Ignore non-defined months. *)
    |> List.traverseResultM id (* Traverses the list accumulating the result, stopping in the first error. *)

let private tryParseMonth (root: NodeChildren) (month: Month) =
    let monthName = month |> Union.caseName |> String.lowercased
    let monthNode = root |> Map.tryFind monthName

    match monthNode with
    | Some(Mapping(_, children)) ->
        result {
            let! entries =
                Union.allCasesOf<MonthEntryType> ()
                |> List.traverseResultM (tryParseEntry children monthName)

            return { Month = month; Entries = entries }
        }
        |> Some
    | Some _ -> Error(InvalidMonth monthName) |> Some
    | None -> None

let private tryParseEntry (monthChildren: NodeChildren) (monthName: string) (entry: MonthEntryType) =
    let entryName = entry |> Union.caseName |> String.lowercased
    let entryNode = monthChildren |> Map.tryFind entryName

    match entryNode with
    | Some(Sequence(_, children)) ->
        result {
            let! fields =
                children
                |> List.map (fun node -> tryParseEntryField node monthName entryName)
                |> List.traverseResultM
                    id (* Traverses the list accumulating the result, stopping in the first error. *)

            return (entry, fields)
        }
    | Some _ -> InvalidEntry(monthName, entryName) |> Error
    | None -> MissingEntry(monthName, entryName) |> Error

let private tryParseEntryField (entryNode: YamlNode) (monthName: string) (entryName: string) =
    match entryNode with
    | Mapping(_, children) ->
        result {
            let! kvp =
                children
                |> List.ofSeq
                |> List.tryHead
                |> Result.fromOption (InvalidField(monthName, entryName))

            let fieldName = kvp.Key
            let fieldValueNode = kvp.Value

            let! value = tryParseEntryValue fieldValueNode monthName entryName fieldName

            return { Name = fieldName; Value = value }
        }
    | _ -> InvalidField(monthName, entryName) |> Error

let private tryParseEntryValue (entryValueNode: YamlNode) (monthName: string) (entryName: string) (fieldName: string) =
    match entryValueNode with
    | Scalar(_, value) ->
        result {
            let! value =
                ScalarParser.Decimal.fromStr value
                |> Result.fromOption (InvalidFieldValue(monthName, entryName, fieldName))

            return value
        }
    | _ -> InvalidFieldValue(monthName, entryName, fieldName) |> Error

// Taken from: https://stackoverflow.com/a/46756368/18076111
let private (|Mapping|Scalar|Sequence|) (yamlNode: YamlNode) =
    match yamlNode.NodeType with
    | YamlNodeType.Mapping ->
        let node = yamlNode :?> YamlMappingNode

        let mapping =
            node.Children
            |> Seq.map (fun kvp ->
                let keyNode = kvp.Key :?> YamlScalarNode
                keyNode.Value, kvp.Value)
            |> Map.ofSeq

        Mapping(node, mapping)
    | YamlNodeType.Scalar ->
        let node = yamlNode :?> YamlScalarNode
        Scalar(node, node.Value)
    | YamlNodeType.Sequence ->
        let node = yamlNode :?> YamlSequenceNode
        Sequence(node, List.ofSeq node.Children)
    | YamlNodeType.Alias
    | _ -> failwith "¯\_(ツ)_/¯"
