// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA1822 // Mark members as static

using System.ComponentModel;
using System.Web.UI.Features;
using Microsoft.AspNetCore.Http.Features;

namespace System.Web.UI;

public class Control : IDisposable
{
    private static readonly Version _renderingCompatibility = new(4, 8, 0);

    internal static readonly object EventDataBinding = new object();
    internal static readonly object EventInit = new object();
    internal static readonly object EventLoad = new object();
    internal static readonly object EventUnload = new object();
    internal static readonly object EventPreRender = new object();
#pragma warning disable CA1823 // Avoid unused private fields
    private static readonly object EventDisposed = new object();
#pragma warning restore CA1823 // Avoid unused private fields

    private EventHandlerList? _events;
    private StateBag? _viewState;
    private ControlCollection? _children;
    private IFeatureCollection? _features;
    private string? _uniqueId;
    private string? _id;

    public string? ClientID => default;
    internal IFeatureCollection Features => _features ??= new FeatureCollection();

    protected virtual ControlCollection CreateControlCollection() => new(this);

    public Control? Parent { get; internal set; }

    public bool Visible { get; set; }

    public virtual void Focus()
    {
    }

    public ControlCollection Controls => _children ??= CreateControlCollection();

    internal StateBag ViewState => _viewState ??= new();

    internal bool HasViewState => _viewState is not null;

    public bool IsTrackingViewState { get; set; }

    public string? ID
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

    private protected void PreventAutoID()
    {
    }

    private protected void SetEnableViewStateInternal(bool enabled)
    {
    }

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

                if (current._children is { } children)
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

    public Control? FindControl(string id)
    {
        foreach (var control in AllChildren)
        {
            if (string.Equals(id, control.ID, StringComparison.OrdinalIgnoreCase))
            {
                return control;
            }
        }

        return null;
    }

    protected EventHandlerList Events
    {
        get
        {
            if (_events == null)
            {
                _events = new EventHandlerList();
            }
            return _events;
        }
    }

    public string? UniqueID
    {
        get
        {
            if (_id is not null)
            {
                return _id;
            }

            if (_uniqueId is null && GetHierarchicalFeature<IUniqueIdGeneratorFeature>() is { } generator)
            {
                _uniqueId = generator.GetUniqueIdGenerator(this);
            }

            return _uniqueId;
        }
    }

    internal bool HasRenderingData() { return HasControls() || HasRenderDelegate(); }

    public virtual bool HasControls() { return Controls != null && Controls.Count > 0; }

    internal bool HasRenderDelegate() => false;

    protected virtual void AddParsedSubObject(object obj)
    {
        if (obj is Control control)
        {
            Controls.Add(control);
        }
    }

    internal bool EnableLegacyRendering => false;

    protected Page? Page => GetHierarchicalFeature<Page>();

    protected HttpContext Context
    {
        get
        {
            if (GetHierarchicalFeature<HttpContext>() is { } context)
            {
                return context;
            }

            throw new NotImplementedException();
        }
    }

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

    protected internal virtual void OnPreRender(EventArgs e)
    {
        if (_events?[EventPreRender] is EventHandler handler)
        {
            handler(this, e);
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

    internal void ValidateEvent(string uniqueID)
    {
        ValidateEvent(uniqueID, string.Empty);
    }

    // Helper function to call validateEvent.
    internal void ValidateEvent(string uniqueID, string eventArgument)
    {
        if (Page != null && SupportsEventValidation)
        {
            Page.ClientScript.ValidateEvent(uniqueID, eventArgument);
        }
    }

    private static bool SupportsEventValidation => true;            //return SupportsEventValidationAttribute.SupportsEventValidation(this.GetType());

    public virtual void Dispose()
    {
    }

    protected virtual bool OnBubbleEvent(object source, EventArgs args)
    {
        return false;
    }

    public string ResolveClientUrl(string relativeUrl) => relativeUrl;

    protected void RaiseBubbleEvent(object source, EventArgs args)
    {
        var currentTarget = Parent;
        while (currentTarget != null)
        {
            if (currentTarget.OnBubbleEvent(source, args))
            {
                return;
            }
            currentTarget = currentTarget.Parent;
        }
    }

    protected virtual void LoadViewState(object savedState)
    {
        if (savedState != null)
        {
            ViewState.LoadViewState(savedState);
        }
    }

    internal void LoadViewStateInternal(object savedState)
        => LoadViewState(savedState);

    internal void RemovedControl(Control child)
    {
        child.Parent = null;
    }

    internal void AddedControl(Control child, int index)
    {
        child.Parent = this;
    }

    internal void ClearNamingContainer()
    {
    }

    internal bool DesignMode => false;

    protected virtual object? SaveViewState()
    {
#if FALSE
        // Save values cached out of view state
        if (flags[visibleDirty])
        {
            ViewState["Visible"] = !flags[invisible];
        }
        if (flags[validateRequestModeDirty])
        {
            ViewState["ValidateRequestMode"] = (int)ValidateRequestMode;
        }
#endif
        return _viewState?.SaveViewState();
    }

    internal Version RenderingCompatibility => _renderingCompatibility;
}
