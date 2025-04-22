#r "nuget: Lestaly, 0.74.0"
#load ".compose-helper.csx"
#nullable enable
using System.Buffers;
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script.
// Install .NET8 and run `dotnet tool install -g dotnet-script`

// deletion of persistent data.

var settings = new
{
    // Service URL
    ServiceUrl = @"http://localhost:9984/",

    // Whether to open the URL after the UP.
    LaunchAfterUp = true,
};

await Paved.RunAsync(async () =>
{
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    WriteLine("Stop service");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes").echo().result().success();
    await "docker".args("volume", "rm", "bookstack-localize-work_bookstack-app-data").echo();
    await "docker".args("volume", "rm", "bookstack-localize-work_bookstack-db-data").echo();

    var volumesDir = ThisSource.RelativeDirectory("./volumes");
    if (volumesDir.Exists)
    {
        WriteLine("Delete volumes files ...");
        volumesDir.DeleteRecurse();
    }

});
