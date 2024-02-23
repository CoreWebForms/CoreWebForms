// MIT License.

using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

/*********************************

Class hierarchy

BaseParser
    DependencyParser
        TemplateControlDependencyParser
            PageDependencyParser
            UserControlDependencyParser
            MasterPageDependencyParser
    TemplateParser
        BaseTemplateParser
            TemplateControlParser
                PageParser
                UserControlParser
                    MasterPageParser
            PageThemeParser
        ApplicationFileParser

**********************************/

namespace System.Web.UI;
// Internal interface for Parser that have exteranl assembly dependency.
internal interface IAssemblyDependencyParser
{
    ICollection AssemblyDependencies { get; }
}

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public partial class BaseParser
{

    // The directory used for relative path calculations
    private VirtualPath _baseVirtualDir;
    internal VirtualPath BaseVirtualDir
    {
        get { return _baseVirtualDir; }

    }

    // The virtual path to the file currently being processed
    private VirtualPath _currentVirtualPath;
    internal VirtualPath CurrentVirtualPath
    {
        get { return _currentVirtualPath; }
        set
        {
            _currentVirtualPath = value;

            // Can happen in the designer
            if (value == null)
            {
                return;
            }

            _baseVirtualDir = value.Parent;
        }
    }

    internal IFileProvider WebFormsFileProvider { get; set; } = default!;

    internal string CurrentVirtualPathString
    {
        get { return System.Web.VirtualPath.GetVirtualPathString(CurrentVirtualPath); }
    }

    internal static readonly Regex TagRegex = new("\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=\"'/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex directiveRegex = new("\\G<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s\"'%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex endtagRegex = new("\\G</(?<tagname>[\\w:\\.]+)\\s*>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex aspCodeRegex = new("\\G<%(?!@)(?<code>.*?)%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex aspExprRegex = new("\\G<%\\s*?=(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex aspEncodedExprRegex = new("\\G<%:(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex databindExprRegex = new("\\G<%#(?<encode>:)?(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex commentRegex = new("\\G<%--(([^-]*)-)*?-%>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex includeRegex = new("\\G\\s*<%\\s*?#(?<encode>:)?(?<code>.*?)?%>\\s*\\z", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex textRegex = new("\\G[^<]+", RegexOptions.Multiline | RegexOptions.Singleline);

    // Regexes used in DetectSpecialServerTagError
    internal static readonly Regex gtRegex = new("[^%]>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex ltRegex = new("<[^%]", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex serverTagsRegex = new("<%(?![#$])(([^%]*)%)*?>", RegexOptions.Multiline | RegexOptions.Singleline);
    internal static readonly Regex runatServerRegex = new("runat\\W*server", RegexOptions.Multiline | RegexOptions.Singleline);

    /*
     * Turns relative virtual path into absolute ones
     */
    internal VirtualPath ResolveVirtualPath(VirtualPath virtualPath)
    {
        return VirtualPathProvider.CombineVirtualPathsInternal(CurrentVirtualPath, virtualPath);
    }
}
