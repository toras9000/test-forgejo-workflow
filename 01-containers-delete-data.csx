#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.69.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

var settings = new
{
    ApiTokenFile = ThisSource.RelativeFile(".auth-forgejo-api"),
};

record TestUserCredential(string Username, string Password);
record TestServiceInfo(string Url, TestUserCredential Admin, string Token);

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    WriteLine("Stop service & delete volume ...");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").result().success();

    WriteLine("Delete auth file ...");
    settings.ApiTokenFile.Delete();
});
