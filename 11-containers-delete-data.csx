#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.79.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

var settings = new
{
    ApiTokenFile = ThisSource.RelativeFile(".auth-forgejo-api"),
};

record TestUserCredential(string Username, string Password);
record TestServiceInfo(string Url, TestUserCredential Admin, string Token);

return await Paved.ProceedAsync(noPause: Args.RoughContains("--nopause"), async () =>
{
    WriteLine("Stop service & delete volume ...");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "--remove-orphans", "--volumes").result().success();

    WriteLine("Delete auth file ...");
    settings.ApiTokenFile.Delete();
});
