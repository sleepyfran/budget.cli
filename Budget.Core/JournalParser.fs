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
    let root = yamlStream.Documents.[0].RootNode

    match root with
    | Mapping(_, children) ->
        result {
            let! year = tryParseYear children

            return
                { Year = year
                  MonthEntries = Map.empty }
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
