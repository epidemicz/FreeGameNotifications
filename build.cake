var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");

var assemblyName = "FreeGameNotifications";
var solution = "./source/FreeGameNotifications.sln";
var output = $"./source/bin/{configuration}";

// List of files to pack, everything else is ignored
List<string> filesToPack = [
    "extension.yaml",
    $"{assemblyName}.dll",
    $"{assemblyName}.pdb",
    "icon.png",
    "en_US.xaml"
];

private (string id, string version) ReadExtensionManifest()
{
    var manifestPath = $"{output}/extension.yaml";
    var manifestLines = System.IO.File.ReadAllLines(manifestPath);

    var id = manifestLines
        .FirstOrDefault(line => line.StartsWith("Id:"))
        ?.Replace("Id: ", string.Empty);

    var version = manifestLines
        .FirstOrDefault(line => line.StartsWith("Version:"))
        ?.Replace("Version: ", string.Empty)
        ?.Replace(".", "_");

    Information("Reading extension.yaml: Id: {0}, Version: {1}", id, version);

    return (id, version);
}

string PackExtension()
{
    // remove all files from output folder except the ones in filesToPack
    var files = System.IO.Directory.GetFiles(output, "*.*", System.IO.SearchOption.AllDirectories);

    Information("Preparing to pack files");
    foreach (var file in files)
    {
        var name = System.IO.Path.GetFileName(file);
        if (!filesToPack.Contains(name))
        {
            Information("File not in filesToPack, removing: {0}", name);
            System.IO.File.Delete(file);
        }
    }

    // Read extension manifest
    (var id, var version) = ReadExtensionManifest();
    var packedExtension = $"{output}/{id}_{version}.pext";

    // Zip contents of output
    Zip(output, packedExtension);

    return packedExtension;
}

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(output);
    });

Task("Restore")
    .Does(() =>
    {
        // Restore NuGet packages
        Information("Restoring NuGet packages...");
        NuGetRestore(solution);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        // Build the solution
        MSBuild(solution, settings =>
        {
            settings.SetConfiguration(configuration);
        });
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(async () =>
    {
        var packedExtension = PackExtension();
        var artifactName = System.IO.Path.GetFileName(packedExtension);

        // upload packedExtension artifact to github
        Information("Packed extension: {0}", artifactName);

        await GitHubActions.Commands.UploadArtifact(new FilePath(packedExtension), assemblyName);
    });

RunTarget(target);