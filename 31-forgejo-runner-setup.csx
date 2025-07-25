#!/usr/bin/env dotnet-script
#r "nuget: Lestaly.General, 0.100.0"
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

    Labels = new[]
    {
        new { Name = "ubuntu",             URI = "docker://ubuntu:24.04",                      },
        new { Name = "ubuntu-24.04",       URI = "docker://ubuntu:24.04",                      },
        new { Name = "ubuntu-22.04",       URI = "docker://ubuntu:22.04",                      },
        new { Name = "debian",             URI = "docker://debian:12",                         },
        new { Name = "debian-12",          URI = "docker://debian:12",                         },
        new { Name = "debian-11",          URI = "docker://debian:11",                         },
        new { Name = "node-debian",        URI = "docker://node:20-bookworm",                  },
        new { Name = "node-20-debian-12",  URI = "docker://node:20-bookworm",                  },
        new { Name = "node-20-debian-11",  URI = "docker://node:20-bullseye",                  },
        new { Name = "docker-cli",         URI = "docker://code.forgejo.org/oci/docker:cli",   },
        new { Name = "dotnet-sdk",         URI = "docker://mcr.microsoft.com/dotnet/sdk:9.0",  },
        new { Name = "dotnet-sdk-9",       URI = "docker://mcr.microsoft.com/dotnet/sdk:9.0",  },
    },
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
    var labels = settings.Labels.Select(l => $"{l.Name}:{l.URI}").JoinString(",");
    await "docker".args("compose", "--file", settings.ComposeFile,
       "run", "--rm", settings.RunnerContainer,
       "forgejo-runner", "register",
            "--no-interactive",
            "--name", settings.RunnerName,
            "--instance", settings.ForgejoURL,
            "--token", runnerToken,
            "--labels", labels
    ).echo().cancelby(signal.Token).result().success();
    WriteLine(Chalk.Green[".. Completed"]);
});
