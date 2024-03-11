// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.ObjectModel;

namespace System.Web.Optimization
{

    /// <summary>
    /// The main entry point for Web optimization bundling and is exposed to developers via <see cref="BundleTable.Bundles"/>.
    /// </summary>
    public class BundleCollection : IEnumerable<Bundle>
    {
        private readonly Dictionary<string, Bundle> _bundles = new Dictionary<string, Bundle>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DynamicFolderBundle> _dynamicBundles = new Dictionary<string, DynamicFolderBundle>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Bundle> _staticBundles = new Dictionary<string, Bundle>(StringComparer.OrdinalIgnoreCase);
        private readonly List<BundleFileSetOrdering> _orderPriority = new List<BundleFileSetOrdering>();
        private readonly IgnoreList _ignoreList = new IgnoreList();
        private readonly IgnoreList _directoryFilter = new IgnoreList();
        private FileExtensionReplacementList _replacementList = new FileExtensionReplacementList();

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleCollection"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor populates <see cref="FileSetOrderList"/>, <see cref="IgnoreList"/>, <see cref="DirectoryFilter"/>, and <see cref="FileExtensionReplacementList"/> 
        /// with default values. These values can be all removed with <see cref="ResetAll"/>.
        /// </remarks>
        public BundleCollection()
        {
            AddDefaultFileExtensionReplacements(FileExtensionReplacementList);
            AddDefaultFileOrderings(FileSetOrderList);
            AddDefaultIgnorePatterns(DirectoryFilter);

            // TODO: We will expose changing the cache in a future release
            Cache = new HttpContextCache();
        }

        // TODO: We will expose changing the cache in a future release
        internal IBundleCache Cache { get; set; }

        /// <summary>
        /// Used to control the sorting of files within a bundle.
        /// </summary>
        public IList<BundleFileSetOrdering> FileSetOrderList
        {
            get
            {
                return _orderPriority;
            }
        }

        /// <summary>
        /// Gets a list of file patterns which are always ignored by a bundle.
        /// </summary>
        /// <remarks>
        /// Files that match a pattern specified by in <see cref="IgnoreList"/> will be ignored even in the case where they are specifically included in the 
        /// bundle definition. To specify ignore patterns that will by applied only to files included using a wildcard or substitution token, use the 
        /// <see cref="DirectoryFilter"/> ignore list.
        /// </remarks>
        public IgnoreList IgnoreList
        {
            get
            {
                return _ignoreList;
            }
        }

        /// <summary>
        /// Gets a list of file patterns which are ignored when including files using wildcards or substitution tokens.
        /// </summary>
        /// <remarks>
        /// This ignore list applies only when using a wildcard character (*) or substitution token as a part of including files in a bundle using <see cref="Bundle.Include(string[])"/>.
        /// or <see cref="Bundle.IncludeDirectory(string,string)"/>.
        /// </remarks>
        public IgnoreList DirectoryFilter
        {
            get
            {
                return _directoryFilter;
            }
        }

        /// <summary>
        /// Enables selecting different permutations of files for different <see cref="OptimizationMode"/> values.
        /// </summary>
        /// <remarks>
        /// The common use case for file extnsion replacement lists is when a script file has a more verbose file (debug.js) to be used during debugging and a 
        /// pre-minified version to be used at runtime (min.js). File extension replacements enables a developer to add all of those files to a bundle with the 
        /// correct file being selected at runtime.
        /// 
        /// Note that the <see cref="FileExtensionReplacementList"/> rules can select files that are not explicitly included in the bundle. For example, 
        /// based on the default file extension conventions, calling <see cref="Bundle.Include(string[])"/> with “jquery.js” will automatically select jquery.min.js 
        /// and jquery.debug.js for the respective <see cref="OptimizationMode"/> if the files exist.
        /// </remarks>
        public FileExtensionReplacementList FileExtensionReplacementList
        {
            get
            {
                return _replacementList;
            }
            set
            {
                _replacementList = value;
            }
        }

        internal Dictionary<string, DynamicFolderBundle> DynamicBundles
        {
            get
            {
                return _dynamicBundles;
            }
        }

        internal Dictionary<string, Bundle> StaticBundles
        {
            get
            {
                return _staticBundles;
            }
        }

        // internal for unit tests, at runtime uses HttpContext.Current
        private HttpContextBase _context;
        internal HttpContextBase Context
        {
            get
            {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
            set
            {
                _context = value;
            }
        }

        /// <summary>
        /// Specifies whether a link to a <see cref="Bundle"/> should attempt to use <see cref="Bundle.CdnPath"/>
        /// </summary>
        public bool UseCdn
        {
            get; set;
        }

        /// <summary>
        /// Adds a bundle to the collection
        /// </summary>
        /// <param name="bundle">The bundle to add to the collection</param>
        public void Add(Bundle bundle)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            var bundlePath = bundle.Path;
            Bundle oldBundle = null;

            // REVIEW: Should we lock down when bundles can be registered?
            var db = bundle as DynamicFolderBundle;
            if (db != null)
            {
                // Remember the old bundle so we can invalidate the cache
                // NOTE: DynamicBundle paths are not virtual paths so we cannot use GetBundleFor
                if (DynamicBundles.ContainsKey(bundlePath))
                {
                    oldBundle = DynamicBundles[bundlePath];
                }

                DynamicBundles[bundlePath] = db;
            }
            else
            {
                // We need to resolve the url since the old bundle could have been a dynamic bundle
                oldBundle = GetBundleFor(bundlePath);

                StaticBundles[bundlePath] = bundle;
            }

            // Invalidate any old cache entries if there was a bundle already registered
            if (oldBundle != null)
            {
                oldBundle.InvalidateCacheEntries();
            }

            _bundles[bundlePath] = bundle;
        }

        /// <summary>
        /// Add default ignore patterns for common conventions.
        /// </summary>
        /// <param name="ignoreList">The <see cref="IgnoreList"/> to populate with default values.</param>
        /// <remarks>
        /// The default ignore patterns added are as follows. Note that file matching these patterns will always be excluded from a bundle, enven if explicitly included. 
        /// To exclude matching files from a bundle only in cases where they are added via a wildcard or substitution token, see <see cref="DirectoryFilter"/>. 
        /// These values can be all removed with IgnoreList.Clear or <see cref="ResetAll"/>.
        /// <list type="bullet">
        ///     <item>
        ///         <description>*.intellisense.js</description>
        ///     </item>
        ///     <item>
        ///         <description>*-vsdoc.js</description>
        ///     </item>
        ///     <item>
        ///         <description>*.debug.js</description>
        ///     </item>
        ///     <item>
        ///         <description>*.min.js</description>
        ///     </item>
        ///     <item>
        ///         <description>*.min.css</description>
        ///     </item>
        ///     <item>
        ///         <description>*.map</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        {
            if (ignoreList == null)
            {
                throw new ArgumentNullException(nameof(ignoreList));
            }
            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
            ignoreList.Ignore("*.map");
        }

        /// <summary>
        /// Add default file ordering for common popuular script and style libraries.
        /// </summary>
        /// <param name="list">A collection of <see cref="BundleFileSetOrdering"/> objects to populate with default values.</param>
        /// <remarks>
        /// The purpose for applying these default file ordering values is to ensure that common libraries such as jquery are always located 
        /// at or close to the top within a bundle. These values can be all removed with <see cref="ResetAll"/>.
        /// 
        /// The default ordering values are as follows:
        /// <list type="bullet">
        ///     <item><description>reset.css</description></item>
        ///     <item><description>normalize.css</description></item>
        ///     <item><description>jquery.js</description></item>
        ///     <item><description>jquery-min.js</description></item>
        ///     <item><description>jquery-*</description></item>
        ///     <item><description>jquery-ui*</description></item>
        ///     <item><description>jquery.ui*</description></item>
        ///     <item><description>jquery.unobtrusive*</description></item>
        ///     <item><description>jquery.validate*</description></item>
        ///     <item><description>modernizr-*</description></item>
        ///     <item><description>dojo.*</description></item>
        ///     <item><description>mootools-core*</description></item>
        ///     <item><description>mootools-*</description></item>
        ///     <item><description>prototype.js</description></item>
        ///     <item><description>prototype-*</description></item>
        ///     <item><description>scriptaculous-*</description></item>
        ///     <item><description>ext.js</description></item>
        ///     <item><description>ext-*</description></item>
        /// </list>
        /// </remarks>
        public static void AddDefaultFileOrderings(IList<BundleFileSetOrdering> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var css = new BundleFileSetOrdering("css");
            css.Files.Add("reset.css");
            css.Files.Add("normalize.css");
            list.Add(css);

            var jquery = new BundleFileSetOrdering("jquery");
            jquery.Files.Add("jquery.js");
            jquery.Files.Add("jquery-min.js");
            jquery.Files.Add("jquery-*");
            jquery.Files.Add("jquery-ui*");
            jquery.Files.Add("jquery.ui*");
            jquery.Files.Add("jquery.unobtrusive*");
            jquery.Files.Add("jquery.validate*");
            list.Add(jquery);

            var mod = new BundleFileSetOrdering("modernizr");
            mod.Files.Add("modernizr-*");
            list.Add(mod);

            var dojo = new BundleFileSetOrdering("dojo");
            dojo.Files.Add("dojo.*");
            list.Add(dojo);

            var moo = new BundleFileSetOrdering("moo");
            moo.Files.Add("mootools-core*");
            moo.Files.Add("mootools-*");
            list.Add(moo);

            var proto = new BundleFileSetOrdering("prototype");
            proto.Files.Add("prototype.js");
            proto.Files.Add("prototype-*");
            proto.Files.Add("scriptaculous-*");
            list.Add(proto);

            var ext = new BundleFileSetOrdering("ext");
            ext.Files.Add("ext.js");
            ext.Files.Add("ext-*");
            list.Add(ext);
        }

        /// <summary>
        /// Add default file extension replacements for common conventions.
        /// </summary>
        /// <param name="list">The <see cref="FileExtensionReplacementList"/> to populate with default values.</param>
        /// <remarks>
        /// The default conventions supported are the following:
        /// <list type="bullet">
        ///     <item><description>select *.min.js when <see cref="BundleTable.EnableOptimizations"/> is true</description></item>
        ///     <item><description>select *.debug.js when <see cref="BundleTable.EnableOptimizations"/> is false</description></item>
        /// </list>
        /// </remarks>
        public static void AddDefaultFileExtensionReplacements(FileExtensionReplacementList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            list.Add("min", OptimizationMode.WhenEnabled);
            list.Add("debug", OptimizationMode.WhenDisabled);
        }

        /// <summary>
        /// Resolves a bundle to a url based on the bundle's virtual path.
        /// </summary>
        /// <param name="bundleVirtualPath">The virtual path for the bundle.</param>
        /// <returns>The bundle url or null if the bundle cannot be found.</returns>
        /// <remarks>
        /// This overload will return a query parameter with a version stamp in the returned bundle url. To control whether the url includes 
        /// a version stamp, use the <see cref="ResolveBundleUrl(string, bool)"/> overload.
        /// </remarks>
        public string ResolveBundleUrl(string bundleVirtualPath)
        {
            return ResolveBundleUrl(bundleVirtualPath, includeContentHash: true);
        }

        /// <summary>
        /// Resolves a bundle to a url based on the bundle's virtual path.
        /// </summary>
        /// <param name="bundleVirtualPath">The virtual path for the bundle.</param>
        /// <param name="includeContentHash">Include a version stamp in the generated url.</param>
        /// <returns>The bundle url or null if the bundle cannot be found.</returns>
        public string ResolveBundleUrl(string bundleVirtualPath, bool includeContentHash)
        {
            var error = ExceptionUtil.ValidateVirtualPath(bundleVirtualPath, "bundleVirtualPath");
            if (error != null)
            {
                throw error;
            }

            var bundle = GetBundleFor(bundleVirtualPath);
            if (bundle == null)
            {
                return null;
            }

            if (UseCdn && !string.IsNullOrEmpty(bundle.CdnPath))
            {
                return bundle.CdnPath;
            }

            return bundle.GetBundleUrl(new BundleContext(Context, this, bundleVirtualPath), includeContentHash);
        }

        /// <summary>
        /// Gets the bundle for a virtual path.
        /// </summary>
        /// <param name="bundleVirtualPath">The virutal path for the bundle.</param>
        /// <returns>The <see cref="Bundle"/> object for the virtual path or null if no bundle exists at the path.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ValidateBundleVirtualPath does validate this argument")]
        public Bundle GetBundleFor(string bundleVirtualPath)
        {
            var error = ExceptionUtil.ValidateVirtualPath(bundleVirtualPath, "bundleVirtualPath");
            if (error != null)
            {
                throw error;
            }

            // Search for exact path match first
            if (StaticBundles.ContainsKey(bundleVirtualPath))
            {
                return StaticBundles[bundleVirtualPath];
            }

            if (DynamicBundles.Count > 0)
            {
                // Otherwise look at virtualPath component for a bundle extension match
                bundleVirtualPath = bundleVirtualPath.Replace("\\", "/");
                var index = bundleVirtualPath.LastIndexOf("/", StringComparison.Ordinal);
                // Note: virtualPath always starts with ~/ so should be never -1;
                var last = bundleVirtualPath.Substring(index + 1);

                if (DynamicBundles.ContainsKey(last))
                {
                    return DynamicBundles[last];
                }
            }
            return null;
        }

        /// <summary>
        /// Removes all the bundles from the collection.
        /// </summary>
        public void Clear()
        {
            _bundles.Clear();
            DynamicBundles.Clear();
            StaticBundles.Clear();
        }

        /// <summary>
        /// Removes all the bundles from the collection and removes support for common conventions.
        /// </summary>
        /// <remarks>
        /// In addition to calling <see cref="Clear"/> on the collection, suport for common conventions is removed by also clearing 
        /// <see cref="FileExtensionReplacementList"/>, <see cref="IgnoreList"/>, <see cref="DirectoryFilter"/>, and <see cref="FileSetOrderList"/>
        /// </remarks>
        public void ResetAll()
        {
            Clear();
            FileExtensionReplacementList.Clear();
            IgnoreList.Clear();
            DirectoryFilter.Clear();
            FileSetOrderList.Clear();
        }

        /// <summary>
        /// Removes a single bundle from the collection.
        /// </summary>
        /// <param name="bundle">The bundle to remove from the collection.</param>
        /// <returns>A boolean value indicating whether the bundle was succesfully removed from the collection.</returns>
        public bool Remove(Bundle bundle)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            var wasRemoved = _bundles.Remove(bundle.Path);
            if (wasRemoved)
            {
                if (bundle is DynamicFolderBundle)
                {
                    DynamicBundles.Remove(bundle.Path);
                }
                else
                {
                    StaticBundles.Remove(bundle.Path);
                }
            }
            return wasRemoved;
        }

        /// <summary>
        /// Gets the number of <see cref="Bundle"/> objects in the collection.
        /// </summary>
        public int Count
        {
            get { return _bundles.Count; }
        }

        /// <summary>
        /// Gets all registered bundles
        /// </summary>
        /// <returns>A read-only collection of all <see cref="Bundle"/> objects in the collection.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Array property that returns a read only list, want users to cache this result")]
        public ReadOnlyCollection<Bundle> GetRegisteredBundles()
        {
            return new ReadOnlyCollection<Bundle>(new List<Bundle>(_bundles.Values));
        }

        /// <summary>
        /// Gets an enumerator for all bundles in the collection.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<Bundle> GetEnumerator()
        {
            return _bundles.Values.GetEnumerator();
        }

        IEnumerator<Bundle> IEnumerable<Bundle>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
