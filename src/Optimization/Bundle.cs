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

    public Bundle Include(params string[] paths)
    {
        _paths.AddRange(paths);
        return this;
    }

    public IEnumerable<string> Paths => _paths;
}
