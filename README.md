# budget.cli

A tiny command line interface to manage a simple budget.

## The what and whys
I've been a user of YNAB for about 5 years now. Loved it for a really long time, but as I got more and more into budgeting
I started to feel like I was unnecessarily tracking way too much. So I had this idea of stripping it out to the fundamentals:
what if I stop tracking every single transaction and just track, at the beginning of the month, how much I've earned and
how I want to distribute it between planned expenses, goals and savings? So I started doing that in a spreadsheet, and
it worked really well, but of course I'm a developer and not being able to seeing my beautiful spreadsheets in a terminal
was a bit of a bummer (said no one ever), so here we are.

## Installation
Right now the only channel to grab the binary is through the [releases page](https://github.com/sleepyfran/budget.cli/releases). There's
a binary for macOS (both Intel and Apple Silicon), and also Linux and Windows, although I've only tested it in macOS.

#### macOS
The binary is not signed, so you'll need to allow it to run by right-clicking on it and selecting `Open`. You'll only need
to do this once, after that you can run it normally (either by double-clicking or from the terminal).

## Usage
The CLI is pretty simple, it accepts a path to a journal file which contains the budget for a year and two options to output
either a monthly or a yearly report. The journal files are just a YAML file with the following structure:

```yaml
year: 2024

january:
  income:
    - salary: 5500
    - opening: 325

  expenses:
    - rent: 1250
    - phone: 35
    - subscriptions: 40
    - credit card: 0
    - extra: 50

  goals:
    - next phone: 250
    - next trip: 1000

  savings:
    - emergency: 1000
    - taxes: 350
    - savings: 250
```

All categories are mandatory, but if you want to keep them empty you can do so by setting it as an empty list (`[]`).

Then, to get a monthly report you can run:

```bash
$ budgetcli journal.yml -m # This outputs the report for the current month
$ budgetcli journal.yml -m january # This outputs the report for january
```

There's also a sample journal included in the app, so if you want to see how it looks like you can run:

```bash
$ budgetcli sample -m # This outputs the report for the current month of the sample journal
$ budgetcli sample -y # This outputs the report for the year of the sample journal
```

The way I use it is by starting a section for the month whenever I get paid, then I fill in the income section
with the amount I got paid (salary), the current balance of my checking account before the payment (opening) and any
other income I might have gotten that month like bonuses (extra). Then I fill in the expenses section with fixed, planned
expenses for the month (rent, phone, subscriptions, etc), and any other expense I might have had planned for the month (extra). Then
I fill in the goals section with whatever I'm currently planning to save for (next phone, next trip, etc), and finally I fill in
the savings section with the amount I want to save for things like emergencies, taxes and general savings. Then, I run the tool
with with the monthly report so that I can see how much I actually have left to spend for the month. I also run it with the yearly
option to see the total amount I've saved for each saving category:

<img width="774" alt="Screenshot showing the month report of a sample budget" src="https://github.com/sleepyfran/budget.cli/assets/6024783/098ce941-9b9c-40ec-a0bd-e48d4005babf">

The idea being that I still try to follow the [first two rules of YNAB](https://www.ynab.com/the-four-rules) without actually
having to track every single transaction. I still have a budget, I still try to give every dollar a job, and I still try to
embrace my true expenses. I just don't track every single transaction, just the big picture.

## Contributing
I've made this mostly for my personal usage, so I'm not really expecting to mold it to anyone else's needs, but if you
think there's definitely something that **needs** to be added, feel free to open a discussion. Worst case scenario, it's
open source, so you can always fork it and do whatever you want with it :^)
