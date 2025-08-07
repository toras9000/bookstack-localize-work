#r "nuget: Lestaly.General, 0.102.0"
#load ".settings.csx"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.RunAsync(async () =>
{
    WriteLine("Stop service");
    await "docker".args("compose", "--file", settings.Docker.Compose, "down", "--remove-orphans", "--volumes").echo().result().success();

    var volumesDir = ThisSource.RelativeDirectory("./volumes");
    if (volumesDir.Exists)
    {
        WriteLine("Delete volumes files ...");
        volumesDir.DeleteRecurse();
    }

    var mailDir = ThisSource.RelativeDirectory("./maildump");
    if (mailDir.Exists)
    {
        WriteLine("Delete maildump files ...");
        mailDir.DeleteRecurse();
    }
});
