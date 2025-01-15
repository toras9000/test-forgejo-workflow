#!/usr/bin/env dotnet-script
#r "nuget: ForgejoApiClient, 10.0.0-rev.1"
#r "nuget: Lestaly, 0.69.0"
#r "nuget: Kokuban, 0.2.0"
#load ".forgejo-token-helper.csx"
#nullable enable
using ForgejoApiClient;
using ForgejoApiClient.Api;
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
    ResourcesDir = ThisSource.RelativeDirectory("test-res"),
    TemplateName = "bake-image",
};

record TestTokenInfo(string Service, string User, string Token);

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    using var signal = new SignalCancellationPeriod();
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Load or Generate token
    var tokenInfo = await settings.Helper.TokenBind(settings.ApiTokenFile);

    WriteLine("Create Forgejo client ...");
    using var forgejo = new ForgejoClient(new(tokenInfo.Service), tokenInfo.Token);
    var apiUser = await forgejo.User.GetMeAsync(cancelToken: signal.Token);
    WriteLine(Chalk.Gray[$"  .. User: {apiUser.login}"]);
    WriteLine();

    WriteLine("Create repository ...");
    var repoPath = $"{settings.TargetUser}/{settings.TemplateName}";
    WriteLine($"  .. {repoPath}");
    var sudoClient = forgejo.Sudo(settings.TargetUser);
    var repos = await sudoClient.Repository.SearchAsync(q: repoPath, cancelToken: signal.Token);
    if (repos.data?.Any(r => r.full_name == repoPath) == true)
    {
        WriteLine(Chalk.Gray[$"  .. Already repository exists."]);
        return;
    }
    var repo = await sudoClient.Repository.CreateAsync(new(name: settings.TemplateName), cancelToken: signal.Token);
    WriteLine(Chalk.Green["  .. Created"]);

    WriteLine("Add files to repository ...");
    var templateDir = settings.ResourcesDir.RelativeDirectory(settings.TemplateName);
    var files = new List<ChangeFileOperation>();
    foreach (var file in templateDir.EnumerateFiles("*", SearchOption.AllDirectories))
    {
        var relPath = file.RelativePathFrom(templateDir, ignoreCase: true).Replace('\\', '/');
        var content = Convert.ToBase64String(file.ReadAllBytes());
        files.Add(new(ChangeFileOperationOperation.Create, path: relPath, content));
    }
    await sudoClient.Repository.UpdateFilesAsync(settings.TargetUser, settings.TemplateName, new(files: files), cancelToken: signal.Token);
    WriteLine(Chalk.Green["  .. Added"]);

    WriteLine("Add branch to repository ...");
    await sudoClient.Repository.CreateBranchAsync(settings.TargetUser, settings.TemplateName, new(new_branch_name: "v0.0.0"), cancelToken: signal.Token);
    WriteLine(Chalk.Green["  .. Added"]);

});
