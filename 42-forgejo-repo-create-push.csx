#!/usr/bin/env dotnet-script
#r "nuget: ForgejoApiClient, 11.0.0-rev.1"
#r "nuget: Lestaly, 0.79.0"
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
    TemplateNames = new[]
    {
        "bake-image",
        "build-nupkg",
        "show-vars",
        "schedule",
        "mirror-oci",
    },
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
    WriteLine(Chalk.Gray[$"  .. User: {apiUser.login}"]);
    WriteLine();

    WriteLine("Create repositories ...");
    foreach (var templateName in settings.TemplateNames)
    {
        var repoPath = $"{settings.TargetUser}/{templateName}";
        WriteLine($"  {repoPath}");
        using var sudoClient = forgejo.Sudo(settings.TargetUser);
        var repos = await sudoClient.Repository.SearchAsync(q: repoPath, cancelToken: signal.Token);
        if (repos.data?.Any(r => r.full_name == repoPath) == true)
        {
            WriteLine(Chalk.Gray[$"  .. Already repository exists."]);
            continue;
        }
        var repo = await sudoClient.Repository.CreateAsync(new(name: templateName), cancelToken: signal.Token);
        WriteLine(Chalk.Green["  .. Created"]);

        WriteLine("  .. Add files to repository ...");
        var templateDir = settings.ResourcesDir.RelativeDirectory(templateName);
        var files = new List<ChangeFileOperation>();
        foreach (var file in templateDir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var relPath = file.RelativePathFrom(templateDir, ignoreCase: true).Replace('\\', '/');
            var content = Convert.ToBase64String(file.ReadAllBytes());
            files.Add(new(ChangeFileOperationOperation.Create, path: relPath, content));
        }
        await sudoClient.Repository.UpdateFilesAsync(settings.TargetUser, templateName, new(files: files), cancelToken: signal.Token);
        WriteLine(Chalk.Green["  .. Added"]);

        WriteLine("  .. Add branch to repository ...");
        await sudoClient.Repository.CreateBranchAsync(settings.TargetUser, templateName, new(new_branch_name: "v0.0.0"), cancelToken: signal.Token);
        WriteLine(Chalk.Green["  .. Added"]);
    }

});
