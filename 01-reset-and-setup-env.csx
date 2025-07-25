#!/usr/bin/env dotnet-script
#r "nuget: Lestaly.General, 0.100.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.ProceedAsync(async () =>
{
    await "dotnet".args("script", "11-containers-delete-data.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "12-containers-restart.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "21-forgejo-initial-setup.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "22-forgejo-create-entities.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "23-forgejo-add-cert-secret.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "24-forgejo-add-test-vars.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "31-forgejo-runner-setup.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "41-forgejo-add-access-token-secret.csx", "--", "--nopause").echo().result().success();
    await "dotnet".args("script", "42-forgejo-repo-create-push.csx", "--", "--nopause").echo().result().success();
});
