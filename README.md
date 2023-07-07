icfpc-2023
==========
codingteam's submission for the ICFP Contest 2023.

https://icfpcontest2023.github.io/
- July 7th, 12:00 UTC (noon) to July 10th, 12:00 UTC

Credentials and Tokens
----------------------
All the credentials and tokens to access the contest resources are stored in a pinned message in our Telegram chat and in the conference topic in the XMPP chat.

⚠ Please save the access token to file `token.txt` in the solution directory.

Prerequisites
-------------
.NET SDK 7.0 is required. Get it for your platform at https://dotnet.microsoft.com/en-us/download (binary installers are available as well as packages for various Linux distributions).

About F#
--------
- https://thomasbandt.com/fsharp-introduction
- https://learnxinyminutes.com/docs/fsharp/
- https://fsharp.github.io/fsharp-core-docs/
- Just ask @ForNeVeR or @gsomix

Build
-----
```console
$ dotnet build
```

Run
---
```console
$ dotnet run --project Icfpc2023 -- [arguments here…]
    # or, alternately:
$ cd Icfpc2023 && dotnet run -- [arguments here…]
```

### Commands

#### HTTP
Download all problems:
```console
$ dotnet run --project Icfpc2023 -- download all
```

Upload all solutions from the `solutions/` directory:
```
$ dotnet run --project Icfpc2023 -- upload all
```

#### Solution
Solve problem 1 using dummy solver:
```console
$ dotnet run --project Icfpc2023 -- solve 1 dummy
```

Calculate score for solution 1 on problem 1:
```console
$ dotnet run --project Icfpc2023 -- score 1
```

#### Oddities
Lambda score showcase (I dunno, see [PR #4](https://github.com/codingteam/icfpc-2023/pull/4/) for details):
```console
$ dotnet run --project Icfpc2023 -- lambdaScore
```

#### Legacy
Download first 3 problems:

```console
$ dotnet run --project Icfpc2023 -- download 3
```

Run Tests
---------
```console
$ dotnet test
```

How to Develop
--------------
See https://dotnet.microsoft.com/en-us/languages/fsharp/tools for the main options.

Fee free to use editor of choice, but the easiest tools to get started are:

- Visual Studio (Windows-only)
  - Community Edition is free and should be okay for this project
  - Open `Infpc2023.sln` and then press the **Run** button

- Visual Studio Code with Ionide extension (cross-platform)
  - Totally free
  - Open the solution folder (the one with the `.sln` file), install the Ionide extension, and then use the **Run and Debug** tool window to execute the project

- JetBrains Rider (cross-platform)
  - Commercial
  - Free EAP and trial builds are available and are okay to use for this project (I, @ForNeVeR, use EAP myself)
  - Open the `.sln` file and then hit the green **Run** button to get started

Other viable options are [Vim](https://github.com/ionide/Ionide-vim), [Emacs](https://github.com/fsharp/emacs-fsharp-mode), and any editor with LSP support via [FsAutoComplete](https://github.com/fsharp/FsAutoComplete).
