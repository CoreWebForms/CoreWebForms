// MIT License.

using System.CommandLine;

var path = new Option<DirectoryInfo>(name: "-p", "Specifies the path to the root directory of the application") { IsRequired = true };
var target = new Argument<DirectoryInfo>("targetDir", "Specifies the path to the root directory of the application");
var rootCommand = new RootCommand("WebForms compilation");

rootCommand.AddOption(path);
rootCommand.AddArgument(target);

rootCommand.SetHandler((path, targetDir) =>
{
    if (!targetDir.Exists)
    {
        targetDir.Create();
    }

    return CompilationHost.RunAsync(path, targetDir);
}, path, target);

await rootCommand.InvokeAsync(args).ConfigureAwait(false);

