#r "nuget: Lestaly, 0.54.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Restart docker container.
// (If it is not activated, it is simply activated.)

var settings = new
{
    // Service URL
    ServiceUrl = @"http://localhost:9984/",

    // Whether to open the URL after the UP.
    LaunchAfterUp = true,
};

await Paved.RunAsync(async () =>
{
    try
    {
        var langDir = ThisSource.RelativeDirectory("./volumes/app/localize/lang");
        var viewsDir = ThisSource.RelativeDirectory("./volumes/app/localize/views");
        if (!langDir.Exists || !viewsDir.Exists)
        {
            Console.WriteLine("Init localize export ...");
            var initFile = ThisSource.RelativeFile("./docker/docker-compose.init.yml");
            await "docker".args("compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes").silent();
            await "docker".args("compose", "--file", initFile.FullName, "run", "app").silent();
            await "docker".args("compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes").silent();
        }

        var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
        Console.WriteLine("Restart service");
        await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").silent();
        await "docker".args("compose", "--file", composeFile.FullName, "up", "-d", "--wait").silent().result().success();

        if (settings.LaunchAfterUp)
        {
            Console.WriteLine("Launch site.");
            await CmdShell.ExecAsync(settings.ServiceUrl);
        }
    }
    catch (CmdProcExitCodeException err)
    {
        throw new PavedMessageException($"ExitCode: {err.ExitCode}\nOutput: {err.Output}", err);
    }
});
