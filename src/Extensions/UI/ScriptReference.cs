// MIT License.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Web.Resources;
using System.Web.Util;
using Microsoft.Extensions.DependencyInjection;
using WebForms.Extensions;
using Debug = System.Diagnostics.Debug;

namespace System.Web.UI
{
    [
    DefaultProperty("Path"),
    ]
    public class ScriptReference : ScriptReferenceBase
    {
        // Maps Tuple<string, Assembly>(resource name, assembly) to string (partial script path)
        private static readonly Hashtable _scriptPathCache = Hashtable.Synchronized(new Hashtable());

        private string _assembly;
        private bool _ignoreScriptPath;
        private string _name;
        private ScriptEffectiveInfo _scriptInfo;

        public ScriptReference() : base() { }

        public ScriptReference(string name, string assembly)
            : this()
        {
            Name = name;
            Assembly = assembly;
        }

        public ScriptReference(string path)
            : this()
        {
            Path = path;
        }

        internal ScriptReference(string name, IClientUrlResolver clientUrlResolver, Control containingControl)
            : this()
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "The script's name must be specified.");
            Debug.Assert(clientUrlResolver != null && clientUrlResolver is ScriptManager, "The clientUrlResolver must be the ScriptManager.");
            Name = name;
            ClientUrlResolver = clientUrlResolver;
            IsStaticReference = true;
            ContainingControl = containingControl;
        }

        internal bool IsDirectRegistration
        {
            // set to true internally to disable checking for adding .debug
            // used when registering a script directly through SM.RegisterClientScriptResource
            get;
            set;
        }

        // We need to map usage of System.Web[.Extensions] to the current assembly. We use typeof here as we're not committed to these
        // names yet and don't want to have to find weird bugs later
        private static readonly string SystemWebAssembly = typeof(Page).Assembly.GetName().Name;
        private static readonly string SystemWebExtensionsAssembly = typeof(ScriptManager).Assembly.GetName().Name;

        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("ScriptReference_Assembly")
        ]
        public string Assembly
        {
            get => _assembly ?? string.Empty;
            set
            {
                if (string.Equals("System.Web", value, StringComparison.OrdinalIgnoreCase))
                {
                    value = SystemWebAssembly;
                }
                else if (string.Equals("System.Web.Extensions", value, StringComparison.OrdinalIgnoreCase))
                {
                    value = SystemWebExtensionsAssembly;
                }

                _assembly = value;
                _scriptInfo = null;
            }
        }

        internal Assembly EffectiveAssembly
        {
            get
            {
                return ScriptInfo.Assembly;
            }
        }

        internal string EffectivePath
        {
            get
            {
                return String.IsNullOrEmpty(Path) ? ScriptInfo.Path : Path;
            }
        }

        internal string EffectiveResourceName
        {
            get
            {
                return ScriptInfo.ResourceName;
            }
        }

        internal ScriptMode EffectiveScriptMode
        {
            get
            {
                if (ScriptMode == ScriptMode.Auto)
                {
                    // - When a mapping.DebugPath exists, ScriptMode.Auto is equivilent to ScriptMode.Inherit,
                    //   since a debug path exists, even though it may not be an assembly based script.
                    // - An explicitly set Path on the ScriptReference effectively ignores a DebugPath.
                    // - When only Path is specified, ScriptMode.Auto is equivalent to ScriptMode.Release.
                    // - When only Name is specified, ScriptMode.Auto is equivalent to ScriptMode.Inherit.
                    // - When Name and Path are both specified, the Path is used instead of the Name, but
                    //   ScriptMode.Auto is still equivalent to ScriptMode.Inherit, since the assumption
                    //   is that if the Assembly contains both release and debug scripts, the Path should
                    //   contain both as well.
                    return ((String.IsNullOrEmpty(EffectiveResourceName) &&
                        (!String.IsNullOrEmpty(Path) || String.IsNullOrEmpty(ScriptInfo.DebugPath))) ?
                        ScriptMode.Release : ScriptMode.Inherit);
                }
                else
                {
                    return ScriptMode;
                }
            }
        }

        [
        Category("Behavior"),
        DefaultValue(false),
        ResourceDescription("ScriptReference_IgnoreScriptPath"),
        Obsolete("This property is obsolete. Instead of using ScriptManager.ScriptPath, set the Path property on each individual ScriptReference.")
        ]
        public bool IgnoreScriptPath
        {
            get
            {
                return _ignoreScriptPath;
            }
            set
            {
                _ignoreScriptPath = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("ScriptReference_Name")
        ]
        public string Name
        {
            get
            {
                return (_name == null) ? String.Empty : _name;
            }
            set
            {
                _name = value;
                _scriptInfo = null;
            }
        }

        internal ScriptEffectiveInfo ScriptInfo
        {
            get
            {
                if (_scriptInfo == null)
                {
                    _scriptInfo = new ScriptEffectiveInfo(this);
                }
                return _scriptInfo;
            }
        }

        private static string AddCultureName(ScriptManager scriptManager, string resourceName)
        {
            Debug.Assert(!String.IsNullOrEmpty(resourceName));
            CultureInfo culture = (scriptManager.EnableScriptLocalization ?
                ScriptReference.DetermineCulture(scriptManager) : CultureInfo.InvariantCulture);
            if (!culture.Equals(CultureInfo.InvariantCulture))
            {
                return AddCultureName(culture, resourceName);
            }
            else
            {
                return resourceName;
            }
        }

        private static string AddCultureName(CultureInfo culture, string resourceName)
        {
            if (resourceName.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                resourceName = string.Concat(resourceName.AsSpan(0, resourceName.Length - 2), culture.Name, ".js");
            }
            return resourceName;
        }

        internal static CultureInfo DetermineCulture(ScriptManager scriptManager)
        {
#if PORT_SCRIPTREFERENCE
            if ((ResourceUICultures == null) || (ResourceUICultures.Length == 0))
            {
                // In this case we want to determine available cultures from assembly info if available
                if (!String.IsNullOrEmpty(EffectiveResourceName))
                {
                    return ScriptResourceHandler
                        .DetermineNearestAvailableCulture(GetAssembly(scriptManager), EffectiveResourceName, CultureInfo.CurrentUICulture);
                }
                return CultureInfo.InvariantCulture;
            }
            CultureInfo currentCulture = CultureInfo.CurrentUICulture;
            while (!currentCulture.Equals(CultureInfo.InvariantCulture))
            {
                string cultureName = currentCulture.ToString();
                foreach (string uiCulture in ResourceUICultures)
                {
                    if (String.Equals(cultureName, uiCulture.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return currentCulture;
                    }
                }
                currentCulture = currentCulture.Parent;
            }
            return currentCulture;
#else
            return CultureInfo.InvariantCulture;
#endif
        }

        internal Assembly GetAssembly()
        {
            return String.IsNullOrEmpty(Assembly) ? null : AssemblyCache.Load(Assembly);
        }

        internal Assembly GetAssembly(ScriptManager scriptManager)
        {
            // normalizes the effective assembly by redirecting it to the given scriptmanager's
            // ajax framework assembly when it is set to SWE.
            // EffectiveAssembly can't do this since ScriptReference does not have access by itself
            // to the script manager.
            Debug.Assert(scriptManager != null);
            Assembly assembly = EffectiveAssembly;
            if (assembly == null)
            {
                return scriptManager.AjaxFrameworkAssembly;
            }
            else
            {
                return ((assembly == AssemblyCache.SystemWebExtensions) ?
                    scriptManager.AjaxFrameworkAssembly :
                    assembly);
            }
        }

        internal string GetPath(ScriptManager scriptManager, string releasePath, string predeterminedDebugPath, bool isDebuggingEnabled)
        {
            // convert the release path to a debug path if:
            // isDebuggingEnabled && not resource based
            // isDebuggingEnabled && resource based && debug resource exists
            // ShouldUseDebugScript is called even when isDebuggingEnabled=false as it verfies
            // the existence of the resource.
            // applies the culture name to the path if appropriate
            string path;
            if (!String.IsNullOrEmpty(EffectiveResourceName))
            {
                Assembly assembly = GetAssembly(scriptManager);
                string resourceName = EffectiveResourceName;
#if PORT_DEBUGRESOURCES
                isDebuggingEnabled = DetermineResourceNameAndAssembly(scriptManager, isDebuggingEnabled, ref resourceName, ref assembly);
#else
                isDebuggingEnabled = false;
#endif
            }

            if (isDebuggingEnabled)
            {
                // Just use predeterminedDebugPath if it is provided. This may be because
                // a script mapping has DebugPath set. If it is empty or null, then '.debug' is added
                // to the .js extension of the release path.
                path = String.IsNullOrEmpty(predeterminedDebugPath) ? GetDebugPath(releasePath) : predeterminedDebugPath;
            }
            else
            {
                path = releasePath;
            }
            return AddCultureName(scriptManager, path);
        }

        internal static Assembly ApplyFallbackResource(Assembly assembly, string releaseName)
        {
#if PORT_SCRIPTREFERENCE
            // fall back to SWE if the assembly does not contain the requested resource
            if ((assembly != AssemblyCache.SystemWebExtensions) &&
                !WebResourceUtil.AssemblyContainsWebResource(assembly, releaseName))
            {
                assembly = AssemblyCache.SystemWebExtensions;
            }
            return assembly;
#else
            return assembly;
#endif
        }

        // Format: <ScriptPath>/<AssemblyName>/<AssemblyVersion>/<ResourceName>
        // This function does not canonicalize the path in any way (i.e. remove duplicate slashes).
        // You must call ResolveClientUrl() on this path before rendering to the page.
        internal static string GetScriptPath(
            string resourceName,
            Assembly assembly,
            CultureInfo culture,
            string scriptPath)
        {

            return scriptPath + "/" + GetScriptPathCached(resourceName, assembly, culture);
        }

        // Cache partial script path, since Version.ToString() and HttpUtility.UrlEncode() are expensive.
        // Increases requests/second by 50% in ScriptManagerScriptPath.aspx test.
        private static string GetScriptPathCached(string resourceName, Assembly assembly, CultureInfo culture)
        {
            Tuple<string, Assembly, CultureInfo> key = Tuple.Create(resourceName, assembly, culture);
            string scriptPath = (string)_scriptPathCache[key];

            if (scriptPath == null)
            {
                // Need to use "new AssemblyName(assembly.FullName)" instead of "assembly.GetName()",
                // since Assembly.GetName() requires FileIOPermission to the path of the assembly.
                // In partial trust, we may not have this permission.
                AssemblyName assemblyName = new AssemblyName(assembly.FullName);

                string name = assemblyName.Name;
                string version = assemblyName.Version.ToString();
                string fileVersion = AssemblyUtil.GetAssemblyFileVersion(assembly);

                if (!culture.Equals(CultureInfo.InvariantCulture))
                {
                    resourceName = AddCultureName(culture, resourceName);
                }

                // Assembly name, fileVersion, and resource name may contain invalid URL characters (like '#' or '/'),
                // so they must be url-encoded.
                scriptPath = String.Join("/", new string[] {
                    HttpUtility.UrlEncode(name), version, HttpUtility.UrlEncode(fileVersion), HttpUtility.UrlEncode(resourceName)
                });

                _scriptPathCache[key] = scriptPath;
            }

            return scriptPath;
        }

        [SuppressMessage("Microsoft.Design", "CA1055", Justification = "Consistent with other URL properties in ASP.NET.")]
        protected internal override string GetUrl(ScriptManager scriptManager, bool zip)
        {
            bool hasName = !String.IsNullOrEmpty(Name);
            bool hasAssembly = !String.IsNullOrEmpty(Assembly);

            if (!hasName && String.IsNullOrEmpty(Path))
            {
                throw new InvalidOperationException(AtlasWeb.ScriptReference_NameAndPathCannotBeEmpty);
            }

            if (hasAssembly && !hasName)
            {
                throw new InvalidOperationException(AtlasWeb.ScriptReference_AssemblyRequiresName);
            }

            return GetUrlInternal(scriptManager, zip);
        }

        internal string GetUrlInternal(ScriptManager scriptManager, bool zip)
        {
            bool enableCdn = scriptManager != null && scriptManager.EnableCdn;
            return GetUrlInternal(scriptManager, zip, useCdnPath: enableCdn);
        }

        internal string GetUrlInternal(ScriptManager scriptManager, bool zip, bool useCdnPath)
        {
            if (!String.IsNullOrEmpty(EffectiveResourceName) && !IsAjaxFrameworkScript(scriptManager) &&
                AssemblyCache.IsAjaxFrameworkAssembly(GetAssembly(scriptManager)))
            {
                // it isnt an AjaxFrameworkScript but it might be from an assembly that is meant to
                // be an ajax script assembly, in which case we should throw an error.
                throw new InvalidOperationException(SR.GetString(AtlasWeb.ScriptReference_ResourceRequiresAjaxAssembly, EffectiveResourceName, GetAssembly(scriptManager)));
            }

            if (!String.IsNullOrEmpty(Path))
            {
                // if an explicit path is set on the SR (not on a mapping) it always
                // takes precedence, even when EnableCdn=true.
                // Also, even if a script mapping has a DebugPath, the explicitly set path
                // overrides all -- so path.debug.js is used instead of the mapping's DebugPath,
                // hence the null 3rd parameter.
                return GetUrlFromPath(scriptManager, Path, null);
            }
            else if (!String.IsNullOrEmpty(ScriptInfo.Path))
            {
                // when only the mapping has a path, CDN takes first priority
                if (useCdnPath)
                {
                    // first determine the actual resource name and assembly to be used
                    // This is so we can (1) apply fallback logic, where ajax fx scripts can come from the 
                    // current ajax assembly or from SWE, whichever is first, and (2) the .debug resource
                    // name is applied if appropriate.
                    string resourceName = EffectiveResourceName;
                    Assembly assembly = null;
                    bool hasDebugResource = false;
                    if (!String.IsNullOrEmpty(resourceName))
                    {
                        assembly = GetAssembly(scriptManager);
#if PORT_DEBUGRESOURCES
                        hasDebugResource = DetermineResourceNameAndAssembly(scriptManager, IsDebuggingEnabled(scriptManager), ref resourceName, ref assembly);
#else
                        hasDebugResource = false;
#endif
                    }
                    string cdnPath = GetUrlForCdn(scriptManager, resourceName, assembly, hasDebugResource);
                    if (!String.IsNullOrEmpty(cdnPath))
                    {
                        return cdnPath;
                    }
                }
                // the mapping's DebugPath applies if it exists
                return GetUrlFromPath(scriptManager, ScriptInfo.Path, ScriptInfo.DebugPath);
            }

            Debug.Assert(!String.IsNullOrEmpty(EffectiveResourceName));
            return GetUrlFromName(scriptManager, scriptManager.Control, zip, useCdnPath);
        }

        private string GetUrlForCdn(ScriptManager scriptManager, string resourceName, Assembly assembly, bool hasDebugResource)
        {
            // if EnableCdn, then url always comes from mapping.Cdn[Debug]Path or WRA.CdnPath, if available.
            // first see if the script description mapping has a cdn path defined
            bool isDebuggingEnabled = IsDebuggingEnabled(scriptManager);
            bool isAssemblyResource = !String.IsNullOrEmpty(resourceName);
            bool secureConnection = scriptManager.IsSecureConnection;
            isDebuggingEnabled = isDebuggingEnabled && (hasDebugResource || !isAssemblyResource);
            string cdnPath = isDebuggingEnabled ?
                (secureConnection ? ScriptInfo.CdnDebugPathSecureConnection : ScriptInfo.CdnDebugPath) :
                (secureConnection ? ScriptInfo.CdnPathSecureConnection : ScriptInfo.CdnPath);

            // then see if the WebResourceAttribute for the resource has one
            // EXCEPT when the ScriptInfo has a cdnpath but it wasn't selected due to this being a secure connection
            // and it does not support secure connections. Avoid having the HTTP cdn path come from the mapping and the
            // HTTPS path come from the WRA.
            if (isAssemblyResource && String.IsNullOrEmpty(cdnPath) && String.IsNullOrEmpty(isDebuggingEnabled ? ScriptInfo.CdnDebugPath : ScriptInfo.CdnPath))
            {
                ScriptResourceInfo scriptResourceInfo = ScriptResourceInfo.GetInstance(assembly, resourceName);
                if (scriptResourceInfo != null)
                {
                    cdnPath = secureConnection ? scriptResourceInfo.CdnPathSecureConnection : scriptResourceInfo.CdnPath;
                }
            }
            return String.IsNullOrEmpty(cdnPath) ? null : ClientUrlResolver.ResolveClientUrl(AddCultureName(scriptManager, cdnPath));
        }

        private string GetUrlFromName(ScriptManager scriptManager, IControl scriptManagerControl, bool zip, bool useCdnPath)
        {
            string resourceName = EffectiveResourceName;
            Assembly assembly = GetAssembly(scriptManager);
            bool hasDebugResource =
#if PORT_DEBUGRESOURCES
                DetermineResourceNameAndAssembly(scriptManager, IsDebuggingEnabled(scriptManager), ref resourceName, ref assembly);
#else
            false;
#endif

            if (useCdnPath)
            {
                string cdnPath = GetUrlForCdn(scriptManager, resourceName, assembly, hasDebugResource);
                if (!String.IsNullOrEmpty(cdnPath))
                {
                    return cdnPath;
                }
            }

            CultureInfo culture = (scriptManager.EnableScriptLocalization ? ScriptReference.DetermineCulture(scriptManager) : CultureInfo.InvariantCulture);
#pragma warning disable 618
            // ScriptPath is obsolete but still functional
            if (IgnoreScriptPath || String.IsNullOrEmpty(scriptManager.ScriptPath))
            {
                return HttpRuntime.WebObjectActivator.GetRequiredService<IScriptResourceHandler>().GetScriptResourceUrl(assembly, resourceName, culture, zip);
            }
            else
            {
                string path = GetScriptPath(resourceName, assembly, culture, scriptManager.ScriptPath);

                if (IsBundleReference)
                {
                    return HttpRuntime.WebObjectActivator.GetRequiredService<IBundleResolver>().GetBundleUrl(path);
                }

                // Always want to resolve ScriptPath urls against the ScriptManager itself,
                // regardless of whether the ScriptReference was declared on the ScriptManager
                // or a ScriptManagerProxy.
                return scriptManagerControl.ResolveClientUrl(path);
            }
#pragma warning restore 618
        }

        private string GetUrlFromPath(ScriptManager scriptManager, string releasePath, string predeterminedDebugPath)
        {
            string path = GetPath(scriptManager, releasePath, predeterminedDebugPath, IsDebuggingEnabled(scriptManager));

            if (IsBundleReference)
            {
                return HttpRuntime.WebObjectActivator.GetRequiredService<IBundleResolver>().GetBundleUrl(path);
            }

            return ClientUrlResolver.ResolveClientUrl(path);
        }

        private bool IsDebuggingEnabled(ScriptManager scriptManager)
        {
#if PORT_SCRIPTMANAGER
            // Deployment mode retail overrides all values of ScriptReference.ScriptMode.
            if (IsDirectRegistration || scriptManager.DeploymentSectionRetail)
            {
                return false;
            }
#endif

            switch (EffectiveScriptMode)
            {
                case ScriptMode.Inherit:
                    return scriptManager.IsDebuggingEnabled;
                case ScriptMode.Debug:
                    return true;
                case ScriptMode.Release:
                    return false;
                default:
                    Debug.Fail("Invalid value for ScriptReference.EffectiveScriptMode");
                    return false;
            }
        }

        protected internal override bool IsAjaxFrameworkScript(ScriptManager scriptManager)
        {
            return (GetAssembly(scriptManager) == scriptManager.AjaxFrameworkAssembly);
        }

        [Obsolete("This method is obsolete. Use IsAjaxFrameworkScript(ScriptManager) instead.")]
        protected internal override bool IsFromSystemWebExtensions()
        {
            return (EffectiveAssembly == AssemblyCache.SystemWebExtensions);
        }

        internal bool IsFromSystemWeb()
        {
            return (EffectiveAssembly == AssemblyCache.SystemWeb);
        }

        // Improves the UI in the VS collection editor, by displaying the Name or Path (if available), or
        // the short type name.
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Name))
            {
                return Name;
            }
            else if (!String.IsNullOrEmpty(Path))
            {
                return Path;
            }
            else
            {
                return GetType().Name;
            }
        }

        internal sealed class ScriptEffectiveInfo
        {
            private readonly string _resourceName;
            private readonly Assembly _assembly;
            private readonly string _path;
            private readonly string _debugPath;
            private readonly string _cdnPath;
            private readonly string _cdnDebugPath;
            private readonly string _cdnPathSecureConnection;
            private readonly string _cdnDebugPathSecureConnection;

            public ScriptEffectiveInfo(ScriptReference scriptReference)
            {
                ScriptResourceDefinition definition =
                    ScriptManager.ScriptResourceMapping.GetDefinition(scriptReference);
                string name = scriptReference.Name;
                string path = scriptReference.Path;
                Assembly assembly = scriptReference.GetAssembly();
                if (definition != null)
                {
                    if (String.IsNullOrEmpty(path))
                    {
                        // only when the SR has no path, the mapping's path and debug path, if any, apply
                        path = definition.Path;
                        _debugPath = definition.DebugPath;
                    }
                    name = definition.ResourceName;
                    assembly = definition.ResourceAssembly;
                    _cdnPath = definition.CdnPath;
                    _cdnDebugPath = definition.CdnDebugPath;
                    _cdnPathSecureConnection = definition.CdnPathSecureConnection;
                    _cdnDebugPathSecureConnection = definition.CdnDebugPathSecureConnection;
                    LoadSuccessExpression = definition.LoadSuccessExpression;
                }
                else if ((assembly == null) && !String.IsNullOrEmpty(name))
                {
                    // name is set and there is no mapping, default to SWE for assembly
                    assembly = AssemblyCache.SystemWebExtensions;
                }
                _resourceName = name;
                _assembly = assembly;
                _path = path;

                if (assembly != null && !String.IsNullOrEmpty(name) && String.IsNullOrEmpty(LoadSuccessExpression))
                {
                    var scriptResourceInfo = ScriptResourceInfo.GetInstance(assembly, name);
                    if (scriptResourceInfo != null)
                    {
                        LoadSuccessExpression = scriptResourceInfo.LoadSuccessExpression;
                    }
                }
            }

            public Assembly Assembly
            {
                get
                {
                    return _assembly;
                }
            }

            public string CdnDebugPath
            {
                get
                {
                    return _cdnDebugPath;
                }
            }

            public string CdnPath
            {
                get
                {
                    return _cdnPath;
                }
            }

            public string CdnDebugPathSecureConnection
            {
                get
                {
                    return _cdnDebugPathSecureConnection;
                }
            }

            public string CdnPathSecureConnection
            {
                get
                {
                    return _cdnPathSecureConnection;
                }
            }

            public string LoadSuccessExpression
            {
                get;
                private set;
            }

            public string DebugPath
            {
                get
                {
                    return _debugPath;
                }
            }

            public string Path
            {
                get
                {
                    return _path;
                }
            }

            public string ResourceName
            {
                get
                {
                    return _resourceName;
                }
            }
        }
    }
}
