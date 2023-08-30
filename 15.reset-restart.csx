#r "nuget: Lestaly, 0.45.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

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
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes", });
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes", });

        var volumesDir = ThisSource.RelativeDirectory("./volumes");
        if (volumesDir.Exists)
        {
            Console.WriteLine("Delete volumes files ...");
            if (volumesDir.Exists) volumesDir.Delete(recursive: true);
        }

        Console.WriteLine("Init localize export ...");
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", initFile.FullName, "run", "app", });
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", initFile.FullName, "down", "--remove-orphans", "--volumes", });

        Console.WriteLine("Start service");
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "up", "-d", });

        if (settings.LaunchAfterUp)
        {
            Console.WriteLine("Waiting for accessible ...");
            using var checker = new HttpClient();
            while (true)
            {
                try { await checker.GetAsync(settings.ServiceUrl); break; }
                catch { await Task.Delay(1000); }
            }

            Console.WriteLine("Launch site.");
            await CmdShell.ExecAsync(settings.ServiceUrl);
        }
    }
    catch (CmdProcExitCodeException err)
    {
        throw new PavedMessageException($"ExitCode: {err.ExitCode}\nOutput: {err.Output}", err);
    }

});
