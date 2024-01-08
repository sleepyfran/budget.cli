module Budget.CLI.Errors

open Budget.Core
open Spectre.Console
open SpectreCoff

/// Returns the formatted error of a given parsing error.
let fromParsing =
    function
    | JournalParser.InvalidSyntax ->
        MarkupC(Color.Red, "There was an error parsing the file. Check that it is valid YAML.")
    | JournalParser.YearMissing ->
        Many
            [ Styles.error "You are missing a year field in your journal. Add it to the top of the file. For example:"
              Styles.error "year: 2021" ]
    | JournalParser.YearInvalid ->
        Many
            [ Styles.error "The year you specified is invalid. Use, you know, a normal year."
              Styles.hint
                  "(I mean, the app is checking for years before 1900 and after 3000, but you're not there, right?" ]
    | JournalParser.InvalidMonth monthName ->
        Styles.error
            $"The month \"{monthName}\" is not a valid month. Make sure you have a valid month name in your journal."
    | JournalParser.MissingEntry(monthName, entryName) ->
        Styles.error $"The entry \"{entryName}\" is missing from the month \"{monthName}\"."
    | JournalParser.InvalidEntry(monthName, entryName) ->
        Styles.error
            $"The entry \"{entryName}\" is invalid in the month \"{monthName}\". Make sure you have a valid entry name in your journal."
    | JournalParser.InvalidField(monthName, entryName) ->
        Many
            [ Styles.error $"A field inside the entry \"{entryName}\" of the month {monthName} is invalid."
              Styles.hint "Make sure you have a all fields inside an entry have the format \"- name: value\"" ]
    | JournalParser.InvalidFieldValue(monthName, entryName, fieldName) ->
        Styles.error
            $"The field \"{fieldName}\", inside the entry \"{entryName}\" of the month \"{monthName}\" contains an invalid value. Make sure the value is a decimal number."
