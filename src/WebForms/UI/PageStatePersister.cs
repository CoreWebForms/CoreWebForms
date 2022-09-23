// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Web.UI;

public abstract class PageStatePersister
{
    private Page _page;
    private IStateFormatter2 _stateFormatter;

    protected PageStatePersister(Page page)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page), SR.GetString(SR.PageStatePersister_PageCannotBeNull));
        }
        _page = page;
    }

    public object ControlState { get; set; }

    /// <devdoc>
    /// Provides the formatter used to serialize and deserialize the object graph representing the
    /// state to be persisted.
    /// </devdoc>
    protected IStateFormatter StateFormatter
    {
        get { return StateFormatter2; }
    }

    internal IStateFormatter2 StateFormatter2
    {
        get
        {
            if (_stateFormatter == null)
            {
                _stateFormatter = Page.CreateStateFormatter();
            }
            return _stateFormatter;
        }
    }

    protected Page Page
    {
        get
        {
            return _page;
        }
        set
        {
            _page = value;
        }
    }

    public object ViewState { get; set; }

    public abstract void Load();

    public abstract void Save();
}
