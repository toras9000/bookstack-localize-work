#r "nuget: Lestaly, 0.57.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script.
// Install .NET8 and run `dotnet tool install -g dotnet-script`

// Restart docker container with deletion of persistent data.
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
        var initFile = ThisSource.RelativeFile("./docker/docker-compose.init.yml");
        var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
        Console.WriteLine("Stop service");
        await "docker".args("compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes").silent();
        await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").silent();

        var volumesDir = ThisSource.RelativeDirectory("./volumes");
        if (volumesDir.Exists)
        {
            Console.WriteLine("Delete volumes files ...");
            if (volumesDir.Exists) volumesDir.Delete(recursive: true);
        }

        Console.WriteLine("Init localize export ...");
        await "docker".args("compose", "--file", initFile.FullName, "run", "app").silent();
        await "docker".args("compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes").silent();

        Console.WriteLine("Start service");
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
