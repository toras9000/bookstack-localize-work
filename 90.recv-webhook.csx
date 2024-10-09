#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: Lestaly, 0.68.0"
#r "nuget: Kokuban, 0.2.0"
#load ".compose-helper.csx"
#nullable enable
using System.IO;
using System.Net.Http;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Kokuban;
using Lestaly;

// This script is meant to run with dotnet-script.
// Install .NET8 and run `dotnet tool install -g dotnet-script`

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
            WriteLine(Chalk.Green[$"{DateTime.Now}: Endpoint={request.Path}, JSON received."]);
            WriteLine(json);
        }
        catch
        {
            WriteLine(Chalk.Yellow[$"{DateTime.Now}: Endpoint={request.Path}, Not JSON."]);
        }

        return Results.Ok();
    });
    server.MapFallback((HttpRequest request) =>
    {
        WriteLine(Chalk.Gray[$"{DateTime.Now}: Ignore request, Path={request.Path}"]);
        return Results.NotFound();
    });

    // Start HTTP Server
    Console.WriteLine($"Start HTTP service.");
    await server.RunAsync($"http://*:{settings.PortNumber}");
});
