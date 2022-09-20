// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        var files = Directory.EnumerateFiles(Path.Combine(GetGitDir(), "src", "WebForms"), "*.cs", new EnumerationOptions { RecurseSubdirectories = true });
        var set = new HashSet<string>();

        foreach (var file in files)
        {
            if (Path.GetFileName(file) == "SR.cs")
            {
                continue;
            }

            var contents = File.ReadAllText(file);
            var matches = regex.Matches(contents);

            foreach (var match in matches.Cast<Match>())
            {
                var name = match.Value.Replace("SR.", string.Empty);
                set.Add(name);
            }
        }

        var sb = new StringBuilder();

        foreach (var item in set.OrderBy(s => s))
        {
            if (item == "GetString")
            {
                continue;
            }

            sb.AppendLine($"public const string {item} = nameof({item});");
        }

        var result = sb.ToString();
    }
}
