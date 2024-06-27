#r "nuget: Lestaly, 0.61.0"
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
    var initFile = ThisSource.RelativeFile("./docker/extract-resource.yml");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    var volumeFile = ThisSource.RelativeFile("./docker/volume-bind.yml");
    Console.WriteLine("Stop service");
    await "docker".args("compose", "--file", composeFile.FullName, "--file", initFile.FullName, "--file", volumeFile.FullName, "down", "--remove-orphans", "--volumes").silent().result().success();

    var volumesDir = ThisSource.RelativeDirectory("./volumes");
    if (volumesDir.Exists)
    {
        Console.WriteLine("Delete volumes files ...");
        volumesDir.DeleteRecurse();
    }

});
