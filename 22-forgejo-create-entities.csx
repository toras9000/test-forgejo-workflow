#!/usr/bin/env dotnet-script
#r "nuget: ForgejoApiClient, 12.0.1-rev.4"
#r "nuget: Lestaly.General, 0.102.0"
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

    Users = new[]
    {
        new { Name = "toras9000", Mail = "toras9000@myserver.home", Pass = "toras9000", },
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
    using (var breaker = signal.Token.CreateLink(TimeSpan.FromSeconds(5)))
    {
        // 初期化直後はAPI呼び出しがエラーとなることがあるようなので、一定時間繰り返し呼び出しを試みる。
        var me = default(User);
        while (me?.login == null)
        {
            try { me = await forgejo.User.GetMeAsync(breaker.Token); }
            catch { await Task.Delay(TimeSpan.FromMilliseconds(500), breaker.Token); }
        }
        WriteLine(Chalk.Green[$"  .. User: {me.login}"]);
    }
    WriteLine();

    WriteLine("Create users ...");
    foreach (var user in settings.Users)
    {
        WriteLine($"  User: {user.Name}");
        var exists = await forgejo.Admin.ListUsersAsync(login_name: user.Name, cancelToken: signal.Token);
        if (0 < exists.Length)
        {
            WriteLine($"  .. {Chalk.Gray["Already exists"]}");
            continue;
        }
        try
        {
            await forgejo.Admin.CreateUserAsync(
                new(
                    email: user.Mail,
                    username: user.Name,
                    @password: user.Pass,
                    must_change_password: false
                ),
                signal.Token
            );
            WriteLine($"  .. {Chalk.Green["Created"]}");
        }
        catch (Exception ex)
        {
            WriteLine($"  .. {Chalk.Red[ex.Message]}");
        }
    }

});
