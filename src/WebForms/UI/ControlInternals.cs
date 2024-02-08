// MIT License.

#nullable enable

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace System.Web.UI;

public partial class Control
{
    private IFeatureCollection? _features;

    internal IFeatureCollection Features => _features ??= new FeatureCollection();

    private protected T? GetHierarchicalFeature<T>()
    {
        if (_features is not null && _features.Get<T>() is { } t)
        {
            return t;
        }

        return Parent is { } p ? p.GetHierarchicalFeature<T>() : default;
    }

    internal bool HasViewState => _viewState is not null;

    protected internal ILogger Logger => Context.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName ?? GetType().Name);
}
