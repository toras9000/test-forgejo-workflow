#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.79.0"
#nullable enable
using Lestaly;

await CmdShell.ExecAsync("https://forgejo.myserver.home");
