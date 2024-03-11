// MIT License.

using System.CodeDom.Compiler;
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

    public static async Task Main()
    {
        var srRegex = new Regex(@"SR\.\w*", RegexOptions.Compiled);
        var assemblyRefRegex = new Regex(@"AssemblyRef\.\w*", RegexOptions.Compiled);

        var dir = Path.Combine(GetGitDir(), "src");
        var srFile = Path.Combine(dir, "WebForms", "SR.cs");

        if (File.Exists(srFile))
        {
            File.Delete(srFile);
        }

        var map = await GetResourceStrings().ConfigureAwait(false);
        var files = Directory.EnumerateFiles(dir, "*.cs", new EnumerationOptions { RecurseSubdirectories = true });
        var sr = new HashSet<string>();
        var assemblyRefs = new HashSet<string>();

        foreach (var file in files)
        {
            if (Path.GetFileName(file) == "SR.cs")
            {
                continue;
            }

            Console.WriteLine(file);

            var contents = File.ReadAllText(file);
            var srMatches = srRegex.Matches(contents);

            foreach (var match in srMatches.Cast<Match>())
            {
                var name = match.Value.Replace("SR.", string.Empty);
                sr.Add(name);
            }

            var assemblymatches = assemblyRefRegex.Matches(contents);
            foreach (var match in assemblymatches.Cast<Match>())
            {
                var name = match.Value.Replace("AssemblyRef.", string.Empty);
                assemblyRefs.Add(name);
            }
        }

        using var fs = File.OpenWrite(srFile);
        using var writer = new StreamWriter(fs);
        using var indented = new IndentedTextWriter(writer);

        indented.WriteLine("// MIT");
        indented.WriteLine("");
        indented.WriteLine();
        indented.WriteLine("namespace System.Web;");
        indented.WriteLine();

        indented.WriteLine("internal static class AssemblyRef");
        indented.WriteLine("{");
        indented.Indent++;

        foreach (var a in assemblyRefs)
        {
            indented.WriteLine($"public const string {a} = nameof({a});");
        }

        indented.Indent--;
        indented.WriteLine("}");
        indented.WriteLine();

        indented.WriteLine("internal static class SR");
        indented.WriteLine("{");

        indented.Indent++;

        indented.WriteLine("public static string GetString(string name) => name;");
        indented.WriteLine();
        indented.WriteLine("public static string GetString(string name, params object[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, name, args);");
        indented.WriteLine();

        foreach (var item in sr.OrderBy(s => s))
        {
            if (item == "GetString")
            {
                continue;
            }

            indented.Write($"public const string {item} = ");

            if (map.TryGetValue(item, out var str))
            {
                indented.Write("\"");
                indented.Write(str);
                indented.WriteLine("\";");
            }
            else
            {
                indented.WriteLine($"nameof({item});");
            }
        }

        indented.Indent--;
        indented.WriteLine("}");
    }

    private static readonly string _path = Path.Combine(Path.GetTempPath(), "system_web_sr_text");

    private static async Task<Dictionary<string, string>> GetResourceStrings()
    {
        var result = new Dictionary<string, string>();

        using var file = await ReadResourceStrings().ConfigureAwait(false);
        using var reader = new StreamReader(file);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            if (line.StartsWith(';'))
            {
                continue;
            }

            var split = line.Split('=');

            if (split.Length == 2)
            {
                result.Add(split[0], split[1].Replace("\"", "\\\""));
            }
        }

        return result;
    }

    private static async Task<Stream> ReadResourceStrings()
    {
        if (File.Exists(_path))
        {
            return File.OpenRead(_path);
        }

        using (var file = File.OpenWrite(_path))
        {
            using var client = new HttpClient();
            using var result = await client.GetAsync("https://raw.githubusercontent.com/microsoft/referencesource/master/System.Web/System.Web.txt").ConfigureAwait(false);

            await result.Content.ReadAsStream().CopyToAsync(file).ConfigureAwait(false);
        }

        return File.OpenRead(_path);
    }
}
