// MIT License.

using System.Collections;
using System.Web.Configuration;

namespace System.Web.UI;
// This used ConfigurationManager in framework
internal class PagesSection
{
    public ICollection<TagNamespaceRegisterEntry> DefaultTagNamespaceRegisterEntries { get; } = new List<TagNamespaceRegisterEntry>();

    public IDictionary IgnoreDeviceFilters { get; } = new Hashtable();

    public string MasterPageFileInternal { get; internal set; } = string.Empty;
    public TagNamespaceRegisterEntryTable TagNamespaceRegisterEntriesInternal { get; } = new();
    public Hashtable UserControlRegisterEntriesInternal { get; } = new();
    public NamespaceCollection Namespaces { get; } = new();
    public TagMappingCollection TagMapping { get; } = new();
    public bool AutoEventWireup { get; internal set; }
    public bool EnableViewState { get; internal set; } = true;
    public CompilationMode CompilationMode { get; internal set; }
    public bool Buffer { get; internal set; }
    public bool EnableViewStateMac { get; internal set; } = true;
    public bool EnableEventValidation { get; internal set; }
    public object ThemeInternal { get; internal set; }
    public string Theme { get; internal set; }
    public string StyleSheetThemeInternal { get; internal set; }
    public ViewStateEncryptionMode ViewStateEncryptionMode { get; internal set; }
    public bool MaintainScrollPositionOnPostBack { get; internal set; }
    public int MaxPageStateFieldLength { get; internal set; } = Page.DefaultMaxPageStateFieldLength;
    public PagesEnableSessionState EnableSessionState { get; internal set; }
    public bool ValidateRequest { get; internal set; }
    public Type PageBaseTypeInternal { get; internal set; }
    public Type UserControlBaseTypeInternal { get; internal set; }

    internal class TagMappingCollection
    {
        public Hashtable TagTypeMappingInternal { get; internal set; }
    }

    internal class NamespaceCollection
    {
        public Hashtable NamespaceEntries { get; internal set; }
        public bool AutoImportVBNamespace => true;
    }

    internal static PageParserFilter CreateControlTypeFilter()
    {
        return null;
    }
}
