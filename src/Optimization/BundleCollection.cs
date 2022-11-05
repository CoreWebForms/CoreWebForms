// MIT License.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Optimization;

public class BundleCollection
{
    private readonly Dictionary<string, Bundle> _bundles = new();

    public void Add(Bundle bundle)
        => _bundles.Add(bundle.Name, bundle);

    internal bool TryGetBundle(string name, [MaybeNullWhen(false)] out Bundle bundle) => _bundles.TryGetValue(name, out bundle);
}
