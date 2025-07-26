#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: Lestaly.General, 0.100.0"
#r "nuget: Kokuban, 0.2.0"
#load ".settings.csx"
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

var whSettings = new
{
    // Accept port for HTTP service.
    PortNumber = 9980,

    // Accept endpoint path
    EndpointName = "test-endpoint",

    // Name of the host as seen from within the container
    ContainerGatewayName = "host.docker.internal",

    // Maximum output length of received JSON. If the value is less than or equal to zero, output the whole.
    MaxJsonOutputLength = -1,
};

return await Paved.ProceedAsync(async () =>
{
    // Set output encoding to UTF8.
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Display URL to set up in BookStack.
    // This will be by the hostname added to the included docker container.
    WriteLine($"Endpoint address:");
    WriteLine($"    http://{whSettings.ContainerGatewayName}:{whSettings.PortNumber}/{whSettings.EndpointName}");
    WriteLine();

    // Formatting options for outputting JSON.
    var jsonOpt = new JsonSerializerOptions();
    jsonOpt.WriteIndented = true;
    jsonOpt.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    // Web Server Configuration Builder
    var builder = WebApplication.CreateBuilder();
    builder.Logging.ClearProviders();

    // Build a server instance
    var server = builder.Build();
    server.MapPost($"/{whSettings.EndpointName}", async (HttpRequest request) =>
    {
        // Endpoint for receiving Webhooks
        try
        {
            var body = await request.ReadFromJsonAsync<JsonElement>();
            var json = JsonSerializer.Serialize(body, jsonOpt);
            if (0 < whSettings.MaxJsonOutputLength) json = json.EllipsisByLength(whSettings.MaxJsonOutputLength, "...");
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
    WriteLine($"Start HTTP service.");
    await server.RunAsync($"http://*:{whSettings.PortNumber}");
});
