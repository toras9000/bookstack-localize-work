#r "nuget: Lestaly, 0.69.0"
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
    WriteLine("Restart service");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans").echo();
    await "docker".args("compose", "--file", composeFile.FullName, "up", "--detach", "--wait").echo().result().success();

    WriteLine("Service address");
    WriteLine($" {Poster.Link[settings.ServiceUrl]}");
    WriteLine();
});
