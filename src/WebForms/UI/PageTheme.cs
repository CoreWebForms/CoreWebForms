// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Web.Util;
using System.Diagnostics;

namespace System.Web.UI;
[EditorBrowsable(EditorBrowsableState.Advanced)]
public abstract class PageTheme
{

    private Page _page;
    private bool _styleSheetTheme;

    protected abstract String[] LinkedStyleSheets { get; }

    protected abstract IDictionary ControlSkins { get; }

    protected abstract String AppRelativeTemplateSourceDirectory { get; }

    protected Page Page
    {
        get
        {
            return _page;
        }
    }

    internal void Initialize(Page page, bool styleSheetTheme)
    {
        Debug.Assert(page != null);
        _page = page;
        _styleSheetTheme = styleSheetTheme;
    }

#if PORT_EVAL
    protected object Eval(string expression)
    {
        return Page.Eval(expression);
    }

    protected string Eval(string expression, string format)
    {
        return Page.Eval(expression, format);
    }
#endif

    public static object CreateSkinKey(Type controlType, String skinID)
    {
        if (controlType == null)
        {
            throw new ArgumentNullException("controlType");
        }

        return new SkinKey(controlType.ToString(), skinID);
    }

    internal void ApplyControlSkin(Control control)
    {
        if (control == null)
        {
            throw new ArgumentNullException("control");
        }

        ControlSkin skin = null;
        String skinId = control.SkinID;
        skin = (ControlSkin)ControlSkins[CreateSkinKey(control.GetType(), skinId)];

        // Don't throw if ControlSkin corresponds to the skinID does not exist.
        Debug.Assert(skin == null || skin.ControlType == control.GetType());

        if (skin != null)
        {
            skin.ApplySkin(control);
        }
    }

    internal void SetStyleSheet()
    {
        if (LinkedStyleSheets != null && LinkedStyleSheets.Length > 0)
        {
            if (Page.Header == null)
                throw new InvalidOperationException(SR.GetString(SR.Page_theme_requires_page_header));

            int index = 0;
            foreach (string styleSheetPath in LinkedStyleSheets)
            {
                HtmlLink link = new HtmlLink();
                link.Href = styleSheetPath;
                link.Attributes["type"] = "text/css";
                link.Attributes["rel"] = "stylesheet";

                if (_styleSheetTheme)
                {
                    Page.Header.Controls.AddAt(index++, link);
                }
                else
                {
                    Page.Header.Controls.Add(link);
                }
            }
        }
    }

    public bool TestDeviceFilter(string deviceFilterName)
    {
        return Page.TestDeviceFilter(deviceFilterName);
    }

#if PORT_XPATH

    protected object XPath(string xPathExpression)
    {
        return Page.XPath(xPathExpression);
    }

    protected object XPath(string xPathExpression, IXmlNamespaceResolver resolver)
    {
        return Page.XPath(xPathExpression, resolver);
    }

    protected string XPath(string xPathExpression, string format)
    {
        return Page.XPath(xPathExpression, format);
    }

    protected string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver)
    {
        return Page.XPath(xPathExpression, format, resolver);
    }

    protected IEnumerable XPathSelect(string xPathExpression)
    {
        return Page.XPathSelect(xPathExpression);
    }

    protected IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver)
    {
        return Page.XPathSelect(xPathExpression, resolver);
    }
#endif

    private class SkinKey
    {
        private string _skinID;
        private string _typeName;

        internal SkinKey(string typeName, string skinID)
        {
            _typeName = typeName;

            if (String.IsNullOrEmpty(skinID))
            {
                _skinID = null;
            }
            else
            {
                _skinID = skinID.ToLower(CultureInfo.InvariantCulture);
            }
        }

        public override int GetHashCode()
        {
            if (_skinID == null)
            {
                return _typeName.GetHashCode();
            }

            return HashCodeCombiner.CombineHashCodes(_typeName.GetHashCode(), _skinID.GetHashCode());
        }

        public override bool Equals(object o)
        {
            SkinKey key = (SkinKey)o;

            return (_typeName == key._typeName) &&
                (_skinID == key._skinID);
        }
    }
}
