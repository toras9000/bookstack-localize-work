#r "nuget: Lestaly, 0.68.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

var containerSettings = new
{
    IniFile = ThisSource.RelativeFile("./docker/extract-resource.yml"),
    ComposeFile = ThisSource.RelativeFile("./docker/compose.yml"),
    VolumeBindFile = ThisSource.RelativeFile("./docker/volume-bind.yml"),
};

CmdCx composeDownAsync()
{
    Span<string> files = [
        "--file", containerSettings.ComposeFile.FullName,
        "--file", containerSettings.IniFile.FullName,
        "--file", containerSettings.VolumeBindFile.FullName,
    ];

    return "docker".args(["compose", .. files, "down", "--remove-orphans", "--volumes"]);
}

CmdCx composeUpAsync(bool volumeBind = false)
    => volumeBind ? "docker".args("compose", "--file", containerSettings.ComposeFile.FullName, "--file", containerSettings.VolumeBindFile.FullName, "up", "-d", "--wait")
                  : "docker".args("compose", "--file", containerSettings.ComposeFile.FullName, "up", "-d", "--wait");

async ValueTask composeRestartAsync(bool volumeBind = false, bool silent = false)
{
    var downCmd = composeDownAsync();
    if (silent) downCmd = downCmd.silent();
    await downCmd;

    var upCmd = composeUpAsync(volumeBind);
    if (silent) upCmd = upCmd.silent();
    await upCmd.result().success();
}

async ValueTask<ushort?> composeGetPublishPort(int app)
{
    var pubPort = await "docker".args("compose", "--file", containerSettings.ComposeFile.FullName, "port", $"app{app}", "80").silent().result().success().output();
    var portNum = pubPort.AsSpan().SkipToken(':').TryParseNumber<ushort>();
    return portNum;
}
