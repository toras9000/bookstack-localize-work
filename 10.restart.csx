#r "nuget: Lestaly, 0.67.0"
#load ".compose-helper.csx"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script.
// Install .NET8 and run `dotnet tool install -g dotnet-script`

// Restart docker container.
// (If it is not activated, it is simply activated.)

var settings = new
{
    // Service URL
    ServiceUrl = @"http://localhost:9984/",

    // Whether to open the URL after the UP.
    LaunchAfterUp = true,
};

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    var langDir = ThisSource.RelativeDirectory("./volumes/app/localize/lang");
    var viewsDir = ThisSource.RelativeDirectory("./volumes/app/localize/views");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    var initFile = ThisSource.RelativeFile("./docker/extract-resource.yml");
    var volumeFile = ThisSource.RelativeFile("./docker/volume-bind.yml");

    await "docker".args("compose", "--file", composeFile.FullName, "--file", initFile.FullName,   "down", "--remove-orphans", "--volumes").silent();
    await "docker".args("compose", "--file", composeFile.FullName, "--file", volumeFile.FullName, "down", "--remove-orphans", "--volumes").silent();

    if (!langDir.Exists || !viewsDir.Exists)
    {
        WriteLine("Init localize export ...");
        await "docker".args("compose", "--file", composeFile.FullName, "--file", initFile.FullName, "run", "app").silent();
        await "docker".args("compose", "--file", composeFile.FullName, "--file", initFile.FullName, "down", "--remove-orphans", "--volumes").silent();
    }

    WriteLine("Restart service");
    await "docker".args("compose", "--file", composeFile.FullName, "--file", volumeFile.FullName, "up", "-d", "--wait").silent().result().success();

    WriteLine("Service address");
    ConsoleWig.Write(" ").WriteLink(settings.ServiceUrl).NewLine();
    WriteLine();
});
