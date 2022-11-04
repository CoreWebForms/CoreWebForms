// MIT License.

using System;
using System.Collections.Generic;

#nullable enable

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler;

internal class HtmlTagNameToTypeMapper
{
    private static QName Create(string name)
    {
        const string HtmlNamespace = "System.Web.UI.HtmlControls";
        return new QName(HtmlNamespace, name);
    }

    public static HtmlTagNameToTypeMapper Instance { get; } = new();

    private readonly Dictionary<string, QName> _tagMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "a", Create("HtmlAnchor") },
        { "button", Create("HtmlButton") },
        { "form", Create("HtmlForm") },
        { "head", Create("HtmlHead") },
        { "img", Create("HtmlImage") },
        { "textarea", Create("HtmlTextArea") },
        { "select", Create("HtmlSelect") },
        { "table", Create("HtmlTable") },
        { "tr", Create("HtmlTableRow") },
        { "td", Create("HtmlTableCell") },
        { "th", Create("HtmlTableCell") },

        // Add new html 5 audio/video tags which resolve the src tag
        { "audio", Create("HtmlAudio") },
        { "video", Create("HtmlVideo") },
        { "track", Create("HtmlTrack") },
        { "source", Create("HtmlSource") },
        { "iframe", Create("HtmlIframe") },
        { "embed", Create("HtmlEmbed") },
        { "area", Create("HtmlArea") },
        { "html", Create("HtmlElement") }
    };

    private readonly Dictionary<string, QName> _inputTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "text", Create("HtmlInputText") },
        { "password", Create("HtmlInputPassword") },
        { "button", Create("HtmlInputButton") },
        { "submit", Create("HtmlInputSubmit") },
        { "reset", Create("HtmlInputReset") },
        { "image", Create("HtmlInputImage") },
        { "checkbox", Create("HtmlInputCheckBox") },
        { "radio", Create("HtmlInputRadioButton") },
        { "hidden", Create("HtmlInputHidden") },
        { "file", Create("HtmlInputFile") }
    };

    public QName GetControlType(string tagName, IReadOnlyDictionary<string, string>? attributeBag)
    {
        if (string.Equals("input", tagName, StringComparison.OrdinalIgnoreCase))
        {
            var type = "text";

            if (attributeBag != null && attributeBag.TryGetValue("type", out var fromAttribute))
            {
                type = fromAttribute;
            }

            if (_inputTypes.TryGetValue(type, out var known))
            {
                return known;
            }
        }
        else if (_tagMap.TryGetValue(tagName, out var fromTag))
        {
            return fromTag;
        }

        // If unknown, use generic
        return Create("HtmlGenericControl");
    }
}

