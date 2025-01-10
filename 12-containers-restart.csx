#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.79.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    WriteLine($"Restart service ...");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "--remove-orphans").echo().result().success();
    await "docker".args("compose", "--file", composeFile, "up", "-d", "--wait").echo().result().success();
    WriteLine("Container up completed.");
});
