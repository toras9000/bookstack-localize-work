#r "nuget: BookStackApiClient, 23.10.0-lib.1"
#r "nuget: SkiaSharp, 2.88.6"
#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Text.RegularExpressions;
using System.Threading;
using BookStackApiClient;
using CometFlavor.Collections;
using Lestaly;
using SkiaSharp;

// This script is meant to run with dotnet-script.
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Create a sample entities.

var settings = new
{
    // API base address for BookStack.(Trailing slash is required.)
    ApiEntry = new Uri(@"http://localhost:9984/api/"),

    // API Token for working container 
    ApiToken = "00001111222233334444555566667777",

    // API Token for working container 
    ApiSecret = "88889999aaaabbbbccccddddeeeeffff",
};

// main processing
await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    // Set output to UTF8 encoding.
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);

    // Handle cancel key press
    using var signal = ConsoleWig.CreateCancelKeyHandlePeriod();

    // Generate a number of entities for testing.
    using var client = new BookStackClient(settings.ApiEntry, settings.ApiToken, settings.ApiSecret);

    // generate sample images
    var images = generateSampleImages();
    var image1 = images[0];
    var image2 = images[1];

    // Create sample entities
    Console.WriteLine("Setup entities ...");
    var book = await client.CreateBookAsync(new("TestBook"), cancelToken: signal.Token);
    var chapter1 = await client.CreateChapterAsync(new(book.id, "TestChapter1"), signal.Token);
    var page1 = await client.CreateMarkdownPageInChapterAsync(new(chapter1.id, "TestPage1", "# markdown page1"), signal.Token);
    var page2 = await client.CreateMarkdownPageInBookAsync(new(book.id, "TestPage2", "# markdown page2"), signal.Token);
    var chapter2 = await client.CreateChapterAsync(new(book.id, "TestChapter2"), signal.Token);
    var page3 = await client.CreateMarkdownPageInChapterAsync(new(chapter2.id, "TestPage3", "# markdown page3"), signal.Token);
    var page4 = await client.CreateMarkdownPageInBookAsync(new(book.id, "TestPage4", "# markdown page4"), signal.Token);
    var shelf = await client.CreateShelfAsync(new("TestShelf", books: new[] { book.id, }), cancelToken: signal.Token);

    var gallery1 = await client.CreateImageAsync(new(page1.id, "gallery", "image1"), image1.Binary, $"image1.{image1.Ext}", signal.Token);
    var gallery2 = await client.CreateImageAsync(new(page2.id, "gallery", "image2"), image2.Binary, $"image2.{image2.Ext}", signal.Token);

    var attach1 = await client.CreateFileAttachmentAsync(new("image1", page3.id), image1.Binary, $"image1.{image1.Ext}", signal.Token);
    var attach2 = await client.CreateFileAttachmentAsync(new("image2", page4.id), image2.Binary, $"image2.{image2.Ext}", signal.Token);
    Console.WriteLine("Completed");

});

record SampleImage(byte[] Binary, string Ext);

List<SampleImage> generateSampleImages()
{
    var images = new List<SampleImage>();
    using var surface = SKSurface.Create(new SKImageInfo(200, 150, SKColorType.Rgba8888));
    var paint = new SKPaint()
    {
        Style = SKPaintStyle.Stroke,
        Color = SKColors.Blue,
        StrokeWidth = 5,
    };
    surface.Canvas.Clear(SKColors.White);
    surface.Canvas.DrawCircle(100f, 75f, 50f, paint);
    using (var image = surface.Snapshot())
    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
    {
        images.Add(new(data.ToArray(), "png"));
    }
    surface.Canvas.Clear(SKColors.White);
    surface.Canvas.DrawRect(25, 25, 150, 100, paint);
    using (var image = surface.Snapshot())
    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 80))
    {
        images.Add(new(data.ToArray(), "jpg"));
    }

    return images;
}