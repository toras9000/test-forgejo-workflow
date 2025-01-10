#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.69.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    WriteLine($"Restart service ...");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans").echo().result().success();
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d", "--wait").echo().result().success();
    WriteLine("Container up completed.");
});
