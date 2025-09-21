#!/usr/bin/env dotnet-script
#r "nuget: Lestaly.General, 0.103.0"
#nullable enable
using Lestaly;

await CmdShell.ExecAsync("https://forgejo.myserver.home");
