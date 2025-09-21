#!/usr/bin/env dotnet-script
#r "nuget: ForgejoApiClient, 12.0.1-rev.4"
#r "nuget: Lestaly.General, 0.103.0"
#r "nuget: Kokuban, 0.2.0"
#load ".forgejo-token-helper.csx"
#nullable enable
using ForgejoApiClient;
using Kokuban;
using Lestaly;
using Lestaly.Cx;

var settings = new
{
    Helper = new ForgejoServiceHelper
    {
        ComposeFile = ThisSource.RelativeFile("./compose.yml"),
        ContainerName = "forgejo",
        ServiceUrl = new Uri("https://forgejo.myserver.home"),
        ApiUser = "forgejo-admin",
    },

    ApiTokenFile = ThisSource.RelativeFile(".auth-forgejo-api"),

    TargetUser = "toras9000",
    TokenName = "PackageWritableToken",
    SecretsName = "PACKAGE_WRITABLE_TOKEN",
};

record TestTokenInfo(string Service, string User, string Token);

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    using var signal = new SignalCancellationPeriod();
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Load or Generate token
    var tokenInfo = await settings.Helper.TokenBind(settings.ApiTokenFile);

    WriteLine("Generate package-write token ...");
    var pkgToken = await settings.Helper.GenerateTokenAsync(settings.TargetUser, settings.TokenName, "write:package");

    WriteLine("Create Forgejo client ...");
    using var forgejo = new ForgejoClient(new(tokenInfo.Service), tokenInfo.Token);
    var apiUser = await forgejo.User.GetMeAsync(cancelToken: signal.Token);
    WriteLine(Chalk.Green[$"  .. User: {apiUser.login}"]);
    WriteLine();

    WriteLine("Set action secret ...");
    using var sudoClient = forgejo.Sudo(settings.TargetUser);
    await sudoClient.User.SetActionsSecretAsync(settings.SecretsName, new(pkgToken));
    WriteLine(Chalk.Green[$"  .. OK"]);

});
