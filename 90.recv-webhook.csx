#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: Lestaly, 0.54.0"
#nullable enable
using Microsoft.AspNetCore.Builder;
using System.Net.Http;
using System.Threading;
using Lestaly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Script to receive and display BookStack webhook submissions.
// The webhook JSON contains a translation string.

var settings = new
{
    // Accept port for HTTP service.
    PortNumber = 9980,

    // Accept endpoint path
    EndpointName = "test-endpoint",

    // Name of the host as seen from within the container
    ContainerGatewayName = "localize-host-gateway",

    // Maximum output length of received JSON. If the value is less than or equal to zero, output the whole.
    MaxJsonOutputLength = -1,
};

await Paved.RunAsync(async () =>
{
    // Set output encoding to UTF8.
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Display URL to set up in BookStack.
    // This will be by the hostname added to the included docker container.
    Console.WriteLine($"Endpoint address:");
    Console.WriteLine($"    http://{settings.ContainerGatewayName}:{settings.PortNumber}/{settings.EndpointName}");
    Console.WriteLine();

    // Formatting options for outputting JSON.
    var jsonOpt = new JsonSerializerOptions();
    jsonOpt.WriteIndented = true;
    jsonOpt.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    // Web Server Configuration Builder
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders();

    // Build a server instance
    var server = builder.Build();
    server.MapPost($"/{settings.EndpointName}", async (HttpRequest request) =>
    {
        // Endpoint for receiving Webhooks
        try
        {
            var body = await request.ReadFromJsonAsync<JsonElement>();
            var json = JsonSerializer.Serialize(body, jsonOpt);
            if (0 < settings.MaxJsonOutputLength) json = json.EllipsisByLength(settings.MaxJsonOutputLength, "...");
            ConsoleWig.WriteLineColored(ConsoleColor.Green, $"{DateTime.Now}: Endpoint={request.Path}, JSON received.").WriteLine(json);
        }
        catch
        {
            ConsoleWig.WriteLineColored(ConsoleColor.Yellow, $"{DateTime.Now}: Endpoint={request.Path}, Not JSON.");
        }

        return Results.Ok();
    });
    server.MapFallback((HttpRequest request) =>
    {
        ConsoleWig.WriteLineColored(ConsoleColor.DarkGray, $"{DateTime.Now}: Ignore request, Path={request.Path}");
        return Results.NotFound();
    });

    // Start HTTP Server
    Console.WriteLine($"Start HTTP service.");
    await server.RunAsync($"http://*:{settings.PortNumber}");
});
