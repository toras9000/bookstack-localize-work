#r "nuget: SmtpServer, 10.0.1"
#r "nuget: MimeKit, 4.12.0"
#r "nuget: Lestaly, 0.76.0"
#r "nuget: Kokuban, 0.2.0"
#load ".compose-helper.csx"
#nullable enable
using System.Buffers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Lestaly;
using Kokuban;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using SmtpServerServiceProvider = SmtpServer.ComponentModel.ServiceProvider;

// This script is meant to run with dotnet-script.
// Install .NET8 and run `dotnet tool install -g dotnet-script`

// Receive and dump mail.

var settings = new
{
    // Name of the host as seen from within the container
    ContainerGatewayName = "host.docker.internal",

    // Accept port for mail service.
    PortNumber = 1025,
};

await Paved.RunAsync(async () =>
{
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);
    using var signal = new SignalCancellationPeriod();

    // Display server information.
    // This has already been configured in the included container, so no additional configuration should be required.
    WriteLine($"Server address : {settings.ContainerGatewayName}");
    WriteLine($"Server port    : {settings.PortNumber}");
    WriteLine();

    // Configure server options.
    var options = new SmtpServerOptionsBuilder()
        .ServerName(settings.ContainerGatewayName)
        .Endpoint(builder => builder.Endpoint(new IPEndPoint(IPAddress.Any, settings.PortNumber)))
        .Build();

    // Prepare service providers.
    var provider = new SmtpServerServiceProvider();
    provider.Add(new FileMessageStore());

    // Start HTTP Server
    WriteLine($"Start mail receiver.");
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
        WriteLine($"  {timestamp:yyyyMMdd_HHmmss.fff}: To={transaction.To.Select(t => $"{t.User}@{t.Host}").JoinString(", ")}");
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
            if (decodedText.IsNotWhite())
            {
                var textFile = this.SaveDir.RelativeFile($"recv_{timestamp:yyyyMMdd_HHmmss.fff}-text.txt");
                await textFile.WriteAllTextAsync(decodedText, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            WriteLine(Chalk.Red[$"    Failed to store. Err={ex.Message}"]);
        }

        return SmtpResponse.Ok;
    }
}