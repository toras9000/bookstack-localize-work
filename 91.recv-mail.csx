#r "nuget: SmtpServer, 9.0.3"
#r "nuget: MimeKit, 4.1.0"
#r "nuget: Lestaly, 0.43.0"
#nullable enable
using System.Buffers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Lestaly;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using SmtpServerServiceProvider = SmtpServer.ComponentModel.ServiceProvider;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Receive and dump mail.

var settings = new
{
    // Name of the host as seen from within the container
    ContainerGatewayName = "localize-host-gateway",

    // Accept port for mail service.
    PortNumber = 1025,
};

await Paved.RunAsync(async () =>
{
    // Set output encoding to UTF8.
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Handle Ctrl+C
    using var signal = ConsoleWig.CreateCancelKeyHandlePeriod();

    // Display server information.
    // This has already been configured in the included container, so no additional configuration should be required.
    Console.WriteLine($"Server address : {settings.ContainerGatewayName}");
    Console.WriteLine($"Server port    : {settings.PortNumber}");
    Console.WriteLine();

    // Configure server options.
    var options = new SmtpServerOptionsBuilder()
        .ServerName(settings.ContainerGatewayName)
        .Endpoint(builder => builder.Endpoint(new IPEndPoint(IPAddress.Any, settings.PortNumber)))
        .Build();

    // Prepare service providers.
    var provider = new SmtpServerServiceProvider();
    provider.Add(new FileMessageStore());

    // Start HTTP Server
    Console.WriteLine($"Start mail receiver.");
    var server = new SmtpServer.SmtpServer(options, provider);
    await server.StartAsync(signal.Token);
});

class FileMessageStore : MessageStore
{
    public FileMessageStore()
    {
        this.SaveDir = ThisSource.RelativeDirectory("mail").WithCreate();
    }

    public DirectoryInfo SaveDir { get; }

    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.Now;
        Console.WriteLine($"  {timestamp:yyyyMMdd_HHmmss.fff}: To={transaction.To.Select(t => $"{t.User}@{t.Host}").JoinString(", ")}");
        try
        {
            // Copy the entire message into the memory stream.
            // This is to match the I/F of MimeKit.
            using var fullMsg = new MemoryStream();
            foreach (var memory in buffer)
            {
                await fullMsg.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
            }

            // Save the entire message to a file.
            fullMsg.Position = 0;
            var fullFile = this.SaveDir.RelativeFile($"recv_{timestamp:yyyyMMdd_HHmmss.fff}.txt");
            using var fullWriter = fullFile.OpenWrite();
            await fullMsg.CopyToAsync(fullWriter, cancellationToken).ConfigureAwait(false);

            // Decode the message and save the text to a file.
            fullMsg.Position = 0;
            var decodedMsg = await MimeKit.MimeMessage.LoadAsync(fullMsg).ConfigureAwait(false);
            var decodedText = decodedMsg.TextBody;
            if (!string.IsNullOrEmpty(decodedText))
            {
                var textFile = this.SaveDir.RelativeFile($"recv_{timestamp:yyyyMMdd_HHmmss.fff}-text.txt");
                await textFile.WriteAllTextAsync(decodedMsg.TextBody, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            ConsoleWig.WriteLineColored(ConsoleColor.Red, $"    Failed to store. Err={ex.Message}");
        }

        return SmtpResponse.Ok;
    }
}