#r "nuget: Lestaly, 0.83.0"
#load ".settings.csx"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    await Task.CompletedTask;
    WriteLine("Service address");
    WriteLine($" {Poster.Link[settings.BookStack.Url]}");
    WriteLine();
});
