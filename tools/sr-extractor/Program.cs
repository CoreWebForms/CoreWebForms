// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public partial class Program
{
    private static string GetGitDir()
    {
        return Inner(AppContext.BaseDirectory);

        static string Inner(string? dir)
        {
            if (dir is null)
            {
                throw new DirectoryNotFoundException("No .git dir found");
            }

            if (Directory.Exists(Path.Combine(dir, ".git")))
            {
                return dir;
            }

            return Inner(Directory.GetParent(dir)?.FullName);
        }
    }

    public static void Main()
    {
        var regex = new Regex(@"SR\.\w*", RegexOptions.Compiled);

        var dir = Path.Combine(GetGitDir(), "src", "WebForms");
        var srFile = Path.Combine(dir, "SR.cs");

        if (File.Exists(srFile))
        {
            File.Delete(srFile);
        }

        var files = Directory.EnumerateFiles(dir, "*.cs", new EnumerationOptions { RecurseSubdirectories = true });
        var set = new HashSet<string>();

        foreach (var file in files)
        {
            if (Path.GetFileName(file) == "SR.cs")
            {
                continue;
            }

            Console.WriteLine(file);

            var contents = File.ReadAllText(file);
            var matches = regex.Matches(contents);

            foreach (var match in matches.Cast<Match>())
            {
                var name = match.Value.Replace("SR.", string.Empty);
                set.Add(name);
            }
        }

        using var fs = File.OpenWrite(srFile);
        using var writer = new StreamWriter(fs);
        using var indented = new IndentedTextWriter(writer);

        indented.WriteLine("// Licensed to the .NET Foundation under one or more agreements.");
        indented.WriteLine("// The .NET Foundation licenses this file to you under the MIT license.");
        indented.WriteLine();
        indented.WriteLine("namespace System.Web;");
        indented.WriteLine();
        indented.WriteLine("internal static class SR");
        indented.WriteLine("{");

        indented.Indent++;
        indented.WriteLine("public static string GetString(string name, params object[] args) => name;");
        indented.WriteLine();

        foreach (var item in set.OrderBy(s => s))
        {
            if (item == "GetString")
            {
                continue;
            }

            indented.WriteLine($"public const string {item} = nameof({item});");
        }

        indented.Indent--;
        indented.WriteLine("}");
    }
}
