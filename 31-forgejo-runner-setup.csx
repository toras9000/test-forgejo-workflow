#!/usr/bin/env dotnet-script
#r "nuget: Lestaly.General, 0.103.0"
#r "nuget: Kokuban, 0.2.0"
#nullable enable
using Kokuban;
using Lestaly;
using Lestaly.Cx;

var settings = new
{
    ComposeFile = ThisSource.RelativeFile("./compose.yml"),

    ForgejoContainer = "forgejo",
    RunnerContainer = "runner",

    ForgejoURL = "https://forgejo.myserver.home",

    RunnerName = "default-runner",

};

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    using var signal = new SignalCancellationPeriod();
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    WriteLine("Get runner token");
    var runnerToken = await "docker".args("compose", "--file", settings.ComposeFile,
        "exec", "--user", "1000", settings.ForgejoContainer,
        "forgejo", "forgejo-cli", "actions", "generate-runner-token"
    ).silent().cancelby(signal.Token).result().success().output(trim: true);
    WriteLine($".. Token: {runnerToken.Trim()}");

    WriteLine("Register runner");
    await "docker".args("compose", "--file", settings.ComposeFile,
       "run", "--rm", settings.RunnerContainer,
       "forgejo-runner", "register",
            "--no-interactive",
            "--name", settings.RunnerName,
            "--instance", settings.ForgejoURL,
            "--token", runnerToken
    ).echo().cancelby(signal.Token).result().success();
    WriteLine(Chalk.Green[".. Completed"]);
});
