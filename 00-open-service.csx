#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.69.0"
#nullable enable
using Lestaly;

await CmdShell.ExecAsync("https://forgejo.myserver.home");
