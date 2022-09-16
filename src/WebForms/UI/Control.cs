// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.UI.Features;
using Microsoft.AspNetCore.Http.Features;

namespace System.Web.UI;

public class Control : IDisposable
{
    private StateBag? _viewState;
    private ControlCollection? _children;
    private IFeatureCollection? _features;
    private string? _uniqueId;
    private string? _id;

    protected string? ID => Id;

    protected string? ClientID => default;
    internal IFeatureCollection Features => _features ??= new FeatureCollection();

    public Control? Parent { get; internal set; }

    public bool Visible { get; set; }

    public ControlCollection Controls => _children ??= new(this);

    protected StateBag ViewState => _viewState ??= new();

    public bool IsTrackingViewState { get; set; }

    public string? Id
    {
        get => _id;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                value = null;
            }

            _id = value;
        }
    }

    public string? UniqueID
    {
        get
        {
            if (_uniqueId is null && GetHierarchicalFeature<IUniqueIdGeneratorFeature>() is { } generator)
            {
                _uniqueId = generator.GetUniqueIdGenerator(this);
            }

            return _uniqueId;
        }
    }

    protected Page? Page => GetHierarchicalFeature<Page>();

    protected HttpContext Context => GetHierarchicalFeature<HttpContext>() ?? throw new NotImplementedException();

    public virtual void RenderControl(HtmlTextWriter writer)
        => Render(writer);

    protected internal virtual void Render(HtmlTextWriter writer)
        => RenderChildren(writer);

    protected internal virtual void RenderChildren(HtmlTextWriter writer)
    {
        foreach (Control child in Controls)
        {
            child.RenderControl(writer);
        }
    }

    private protected T? GetHierarchicalFeature<T>()
    {
        if (_features is not null && _features.Get<T>() is { } t)
        {
            return t;
        }

        if (Parent is { } p)
        {
            return p.GetHierarchicalFeature<T>();
        }

        return default;
    }

    public virtual void Dispose()
    {
    }
}
