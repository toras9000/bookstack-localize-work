#r "nuget: Lestaly.General, 0.102.0"
#load ".settings.csx"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.ProceedAsync(async () =>
{
    WriteLine("Restart service");
    await "docker".args("compose", "--file", settings.Docker.Compose, "down", "--remove-orphans").echo();
    await "docker".args("compose", "--file", settings.Docker.Compose, "up", "--detach", "--wait").echo().result().success();

    WriteLine();
    await "dotnet".args("script", ThisSource.RelativeFile("12.meke-api-token.csx"), "--", "--no-pause").result().success();
    WriteLine();
    await "dotnet".args("script", ThisSource.RelativeFile("11.show-url.csx"), "--", "--no-pause").result().success();
});
