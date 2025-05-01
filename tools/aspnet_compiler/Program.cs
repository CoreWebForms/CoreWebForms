// MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Options;
using WebForms.Compiler;

var path = new Option<DirectoryInfo>(name: "-p", "Specifies the path to the root directory of the application") { IsRequired = true };
var references = new Option<FileInfo[]>(name: "-r", "Specifies the path to the root directory of the application") { IsRequired = false };
var target = new Argument<DirectoryInfo>("targetDir", "Specifies the path to the root directory of the application");
var isDebug = new Option<bool>("-d", () => false, "Specifies if a debug build");
var prefixes = new Option<string[]>(name: "--prefix", "Specifies the tag prefix in the format: prefix!namespace!assembly");
var isVerbose = new Option<bool>(name: "-v", "Specifies if should be verbose");
var rootCommand = new RootCommand("WebForms compilation");

rootCommand.AddOption(path);
rootCommand.AddOption(isDebug);
rootCommand.AddOption(references);
rootCommand.AddOption(prefixes);
rootCommand.AddOption(isVerbose);
rootCommand.AddArgument(target);

rootCommand.SetHandler((path, targetDir, references, prefixes, isDebug, isVerbose) =>
{
    if (!targetDir.Exists)
    {
        targetDir.Create();
    }

    return CompilationHost.RunAsync(path, targetDir, references, prefixes, isDebug, isVerbose);
}, path, target, references, prefixes, isDebug, isVerbose);

await rootCommand.InvokeAsync(args).ConfigureAwait(false);

