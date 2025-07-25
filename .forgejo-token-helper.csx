#!/usr/bin/env dotnet-script
#r "nuget: Lestaly, 0.100.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

public record ForgejoTokenInfo(string Service, string User, string Token);

public class ForgejoServiceHelper
{
    public required FileInfo ComposeFile { get; init; }
    public required string ContainerName { get; init; }
    public required Uri ServiceUrl { get; init; }
    public required string ApiUser { get; init; }
    public Action<string>? Logger { get; init; }

    public async ValueTask<string> GenerateTokenAsync(string user, string name, string scopes = "all")
    {
        var token = await "docker".args([
            "compose", "--file", this.ComposeFile.FullName, "exec", "-u", "1000", this.ContainerName,
            "forgejo", "admin", "user", "generate-access-token",
                "--raw",
                "--username", user,
                "--token-name", name,
                "--scopes", scopes
        ]).silent().result().success().output();
        return token.Trim();
    }

    public async ValueTask<ForgejoTokenInfo> TokenBind(FileInfo tokenFile)
    {
        this.Logger?.Invoke("Load token info ...");
        var scrambler = tokenFile.CreateScrambler(context: tokenFile.FullName);
        var tokenInfo = await scrambler.DescrambleObjectAsync<ForgejoTokenInfo>();
        if (tokenInfo == null || tokenInfo.Service != this.ServiceUrl.AbsoluteUri || tokenInfo.User != this.ApiUser)
        {
            this.Logger?.Invoke(" .. no valid info");
            this.Logger?.Invoke("Generate access token ...");
            var tokenName = $"test-token-{Guid.NewGuid()}";
            var token = await GenerateTokenAsync(this.ApiUser, tokenName, "all");
            this.Logger?.Invoke($" .. generated: {tokenName}");

            tokenInfo = new(this.ServiceUrl.AbsoluteUri, this.ApiUser, token);
            await scrambler.ScrambleObjectAsync(tokenInfo);
        }
        return tokenInfo;
    }

}
