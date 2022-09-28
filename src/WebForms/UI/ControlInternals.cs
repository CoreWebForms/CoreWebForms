// MIT License.

#nullable enable

using Microsoft.AspNetCore.Http.Features;

namespace System.Web.UI;

public partial class Control
{
    private IFeatureCollection? _features;

    internal IFeatureCollection Features => _features ??= new FeatureCollection();

    internal IEnumerable<Control> AllChildren
    {
        get
        {
            var queue = new Queue<Control>(5);
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                yield return current;

                if (current._controls is { } children)
                {
                    foreach (var child in children)
                    {
                        if (child is Control childControl)
                        {
                            queue.Enqueue(childControl);
                        }
                    }
                }
            }
        }
    }

    private protected T? GetHierarchicalFeature<T>()
    {
        if (_features is not null && _features.Get<T>() is { } t)
        {
            return t;
        }

        return Parent is { } p ? p.GetHierarchicalFeature<T>() : default;
    }

    internal bool HasViewState => _viewState is not null;
}
