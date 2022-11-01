// MIT License.

namespace System.Web.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;

/// <internalonly/>
public sealed class DesignTimeParseData
{

    private readonly IDesignerHost _designerHost;

    private string _documentUrl;

    private EventHandler _dataBindingHandler;

    private readonly string _parseText;

    private readonly string _filter;

    private bool _shouldApplyTheme;

    private ICollection _userControlRegisterEntries;

    public DesignTimeParseData(IDesignerHost designerHost, string parseText) : this(designerHost, parseText, String.Empty)
    {
    }

    public DesignTimeParseData(IDesignerHost designerHost, string parseText, string filter)
    {

        // note that designerHost can be null, we continue on without using any designer-specific services.
        if (String.IsNullOrEmpty(parseText))
        {
            throw new ArgumentNullException(nameof(parseText));
        }

        _designerHost = designerHost;
        _parseText = parseText;
        _filter = filter;
    }

    public bool ShouldApplyTheme
    {
        get
        {
            return _shouldApplyTheme;
        }
        set
        {
            _shouldApplyTheme = value;
        }
    }

    public EventHandler DataBindingHandler
    {
        get
        {
            return _dataBindingHandler;
        }
        set
        {
            _dataBindingHandler = value;
        }
    }

    public IDesignerHost DesignerHost
    {
        get
        {
            return _designerHost;
        }
    }

    public string DocumentUrl
    {
        get
        {
            if (_documentUrl == null)
            {
                return String.Empty;
            }

            return _documentUrl;
        }
        set
        {
            _documentUrl = value;
        }
    }

    public string Filter
    {
        get
        {
            if (_filter == null)
            {
                return String.Empty;
            }

            return _filter;
        }
    }

    public string ParseText
    {
        get
        {
            return _parseText;
        }
    }

    public ICollection UserControlRegisterEntries
    {
        get
        {
            return _userControlRegisterEntries;
        }
    }

    internal void SetUserControlRegisterEntries(ICollection userControlRegisterEntries, List<TagNamespaceRegisterEntry> tagRegisterEntries)
    {
        if (userControlRegisterEntries == null && tagRegisterEntries == null)
        {
            return;
        }

        List<Triplet> allEntries = new List<Triplet>();
        if (userControlRegisterEntries != null)
        {
            foreach (UserControlRegisterEntry entry in userControlRegisterEntries)
            {
                allEntries.Add(new Triplet(entry.TagPrefix, new Pair(entry.TagName, entry.UserControlSource.ToString()), null));
            }
        }
        if (tagRegisterEntries != null)
        {
            foreach (TagNamespaceRegisterEntry entry in tagRegisterEntries)
            {
                allEntries.Add(new Triplet(entry.TagPrefix, null, new Pair(entry.Namespace, entry.AssemblyName)));
            }
        }

        _userControlRegisterEntries = allEntries;
    }
}

