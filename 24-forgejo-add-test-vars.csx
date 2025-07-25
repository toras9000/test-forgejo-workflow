#!/usr/bin/env dotnet-script
#r "nuget: ForgejoApiClient, 12.0.1-rev.1"
#r "nuget: Lestaly.General, 0.100.0"
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
    VariableName = "TEST_VERIABLE",
};

record TestTokenInfo(string Service, string User, string Token);

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    using var signal = new SignalCancellationPeriod();
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Load or Generate token
    var tokenInfo = await settings.Helper.TokenBind(settings.ApiTokenFile);

    WriteLine("Create Forgejo client ...");
    using var forgejo = new ForgejoClient(new(tokenInfo.Service), tokenInfo.Token);
    var apiUser = await forgejo.User.GetMeAsync(cancelToken: signal.Token);
    WriteLine(Chalk.Green[$"  .. User: {apiUser.login}"]);
    WriteLine();

    WriteLine("Set action variable ...");
    using var sudoClient = forgejo.Sudo(settings.TargetUser);
    var content = "test-variable-content";
    await sudoClient.User.CreateActionVariableAsync(settings.VariableName, new(content));
    WriteLine(Chalk.Green[$"  .. OK"]);

});
