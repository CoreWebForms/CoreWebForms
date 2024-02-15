// MIT License.

using System.CommandLine;
using WebForms.Compiler;

var path = new Option<DirectoryInfo>(name: "-p", "Specifies the path to the root directory of the application") { IsRequired = true };
var references = new Option<FileInfo[]>(name: "-r", "Specifies the path to the root directory of the application") { IsRequired = false };
var target = new Argument<DirectoryInfo>("targetDir", "Specifies the path to the root directory of the application");
var isDebug = new Option<bool>("-d", () => false, "Specifies if a debug build");
var rootCommand = new RootCommand("WebForms compilation");

rootCommand.AddOption(path);
rootCommand.AddOption(isDebug);
rootCommand.AddOption(references);
rootCommand.AddArgument(target);

rootCommand.SetHandler((path, targetDir, references, isDebug) =>
{
    if (!targetDir.Exists)
    {
        targetDir.Create();
    }

    return CompilationHost.RunAsync(path, targetDir, references, isDebug);
}, path, target, references, isDebug);

await rootCommand.InvokeAsync(args).ConfigureAwait(false);

