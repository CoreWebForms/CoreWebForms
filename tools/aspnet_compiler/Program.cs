// MIT License.

using System.CommandLine;
using WebForms.Compiler;

var path = new Option<DirectoryInfo>(name: "-p", "Specifies the path to the root directory of the application") { IsRequired = true };
var references = new Option<FileInfo[]>(name: "-r", "Specifies the path to the root directory of the application") { IsRequired = false };
var target = new Argument<DirectoryInfo>("targetDir", "Specifies the path to the root directory of the application");
var rootCommand = new RootCommand("WebForms compilation");

rootCommand.AddOption(path);
rootCommand.AddOption(references);
rootCommand.AddArgument(target);

rootCommand.SetHandler((path, targetDir, references) =>
{
    if (!targetDir.Exists)
    {
        targetDir.Create();
    }

    return CompilationHost.RunAsync(path, targetDir, references);
}, path, target, references);

await rootCommand.InvokeAsync(args).ConfigureAwait(false);

