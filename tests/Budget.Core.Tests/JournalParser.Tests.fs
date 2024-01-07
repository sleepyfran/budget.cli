module Budget.Core.Tests.JournalParser

open Budget.Core.Tests.Utils
open FsUnit.Xunit
open Xunit

open Budget.Core
open Budget.Core.Model

module Basics =
    [<Fact>]
    let ``parsing an empty file results in invalid syntax`` () =
        JournalParser.parse ""
        |> Result.unwrapError
        |> should equal JournalParser.InvalidSyntax

    [<Fact>]
    let ``parsing a file with only comments results in invalid syntax`` () =
        JournalParser.parse "# comment"
        |> Result.unwrapError
        |> should equal JournalParser.InvalidSyntax

    [<Fact>]
    let ``parsing a file that does not start with a mapping node results in invalid syntax`` () =
        JournalParser.parse "0"
        |> Result.unwrapError
        |> should equal JournalParser.InvalidSyntax

module Year =
    [<Fact>]
    let ``parsing a file that does not contain a year results in year missing`` () =
        JournalParser.parse "foo: bar"
        |> Result.unwrapError
        |> should equal JournalParser.YearMissing

    [<Fact>]
    let ``parsing a file that contains a year with invalid characters result in year invalid`` () =
        JournalParser.parse "year: bar"
        |> Result.unwrapError
        |> should equal JournalParser.YearInvalid

    [<Fact>]
    let ``parsing a file that contains a year before 1900 results in year invalid`` () =
        JournalParser.parse "year: 1899"
        |> Result.unwrapError
        |> should equal JournalParser.YearInvalid

    [<Fact>]
    let ``parsing a file that contains a year after 3000 results in year invalid`` () =
        JournalParser.parse "year: 3001"
        |> Result.unwrapError
        |> should equal JournalParser.YearInvalid

    [<Fact>]
    let ``parsing a file with just a valid year results in a journal with no entries and just the year`` () =
        JournalParser.parse "year: 2024"
        |> Result.unwrap
        |> should equal { Year = 2024; MonthEntries = [] }

module Months =
    [<Fact>]
    let ``parsing a file with an invalid month ignores it`` () =
        JournalParser.parse
            """
year: 2024

movember: bar
"""
        |> Result.unwrap
        |> should equal { Year = 2024; MonthEntries = [] }


module Entries =
    [<Fact>]
    let ``parsing a file with a valid month but no entries inside results in in invalid month`` () =
        JournalParser.parse
            """
year: 2024

january:
"""
        |> Result.unwrapError
        |> should equal (JournalParser.InvalidMonth("january"))

    [<Fact>]
    let ``parsing a file with a valid month and not all entry types inside results in missing entry`` () =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - foo: 1000
"""
        |> Result.unwrapError
        |> should equal (JournalParser.MissingEntry("january", "expenses"))

    [<Fact>]
    let ``parsing a file with a valid month and a valid entry type but with invalid content results in invalid entry``
        ()
        =
        JournalParser.parse
            """
year: 2024

january:
    income:
"""
        |> Result.unwrapError
        |> should equal (JournalParser.InvalidEntry("january", "income"))

module Fields =
    [<Fact>]
    let ``parsing a file with a valid month, a valid entry but an invalid field results in invalid field`` () =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - 0
"""
        |> Result.unwrapError
        |> should equal (JournalParser.InvalidField("january", "income"))

    [<Fact>]
    let ``parsing a file with a valid month, a valid entry but an invalid field value results in invalid field value``
        ()
        =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - foo: bar
"""
        |> Result.unwrapError
        |> should equal (JournalParser.InvalidFieldValue("january", "income", "foo"))

    [<Fact>]
    let ``parsing a file with everything valid in one month results in a journal with only that month added`` () =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - salary: 1000
    - extra: 40
    
    expenses:
    - rent: 200
    
    goals:
    - something: 10
    
    savings:
    - emergency: 100
"""
        |> Result.unwrap
        |> should
            equal
            { Year = 2024
              MonthEntries =
                [ { Month = January
                    Entries =
                      [ (Income, [ { Name = "salary"; Value = 1000m }; { Name = "extra"; Value = 40m } ])
                        (Expenses, [ { Name = "rent"; Value = 200m } ])
                        (Goals, [ { Name = "something"; Value = 10m } ])
                        (Savings, [ { Name = "emergency"; Value = 100m } ]) ] } ] }

    [<Fact>]
    let ``parsing a file with everything valid and multiple months results in a journal with all months added`` () =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - salary: 1000
    - extra: 40
    
    expenses:
    - rent: 200
    
    goals:
    - something: 10
    
    savings:
    - emergency: 100
    
february:
    income:
    - salary: 800
    - extra: 20
    
    expenses:
    - rent: 200
    
    goals:
    - something: 10
    
    savings:
    - emergency: 50
"""
        |> Result.unwrap
        |> should
            equal
            { Year = 2024
              MonthEntries =
                [ { Month = January
                    Entries =
                      [ (Income, [ { Name = "salary"; Value = 1000m }; { Name = "extra"; Value = 40m } ])
                        (Expenses, [ { Name = "rent"; Value = 200m } ])
                        (Goals, [ { Name = "something"; Value = 10m } ])
                        (Savings, [ { Name = "emergency"; Value = 100m } ]) ] }
                  { Month = February
                    Entries =
                      [ (Income, [ { Name = "salary"; Value = 800m }; { Name = "extra"; Value = 20m } ])
                        (Expenses, [ { Name = "rent"; Value = 200m } ])
                        (Goals, [ { Name = "something"; Value = 10m } ])
                        (Savings, [ { Name = "emergency"; Value = 50m } ]) ] } ] }

    [<Fact>]
    let ``parsing a file with an empty section in a month is supported`` () =
        JournalParser.parse
            """
year: 2024

january:
    income:
    - salary: 1000
    - extra: 40
    
    expenses:
    - rent: 200
    
    goals: []
    
    savings:
    - emergency: 100
"""
        |> Result.unwrap
        |> should
            equal
            { Year = 2024
              MonthEntries =
                [ { Month = January
                    Entries =
                      [ (Income, [ { Name = "salary"; Value = 1000m }; { Name = "extra"; Value = 40m } ])
                        (Expenses, [ { Name = "rent"; Value = 200m } ])
                        (Goals, [])
                        (Savings, [ { Name = "emergency"; Value = 100m } ]) ] } ] }
