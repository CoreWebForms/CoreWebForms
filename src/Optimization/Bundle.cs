// MIT License.

namespace System.Web.Optimization;

public class Bundle
{
    private readonly List<string> _paths = new();

    public Bundle(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public void Include(params string[] paths)
        => _paths.AddRange(paths);

    public IEnumerable<string> Paths => _paths;
}
