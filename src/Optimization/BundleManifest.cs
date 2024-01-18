// MIT License.

using System.Xml;
using System.Xml.Schema;

namespace System.Web.Optimization;

/// <summary>
/// Enables building a <see cref="BundleCollection"/> from an XML manifest file.
/// </summary>
public sealed class BundleManifest
{
    private const string XsdResourceName = "System.Web.Optimization.BundleManifestSchema.xsd";
    private const string DefaultBundlePath = "~/bundle.config";

    private BundleManifest()
    {
    }

    /// <summary>
    /// Gets the <see cref="StyleBundle"/> objects specified by the manifest file.
    /// </summary>
    public IList<BundleDefinition> StyleBundles { get; private set; } = [];

    /// <summary>
    /// Gets the <see cref="ScriptBundle"/> objects specified by the manifest file.
    /// </summary>
    public IList<BundleDefinition> ScriptBundles { get; private set; } = [];

    /// <summary>
    /// Creates a bundle manifest object from a bundle manifest.
    /// </summary>
    /// <param name="bundleStream">The <see cref="Stream"/> object reading the manifest file.</param>
    /// <returns>The <see cref="BundleManifest"/> object representing the manifest file.</returns>
    public static BundleManifest ReadBundleManifest(Stream bundleStream)
    {
        var document = GetXmlDocument(bundleStream);
        var manifest = new BundleManifest();

        manifest.StyleBundles = document.SelectNodes(@"bundles/styleBundle")
                                    ?.Cast<XmlElement>()
                                    .Select(ReadBundle)
                                    .ToList() ?? [];
        manifest.ScriptBundles = document.SelectNodes(@"bundles/scriptBundle")
                                    ?.Cast<XmlElement>()
                                    .Select(ReadBundle)
                                    .ToList() ?? [];
        return manifest;
    }

    /// <summary>
    /// Creates a bundle manifest object from a bundle manifest.
    /// </summary>
    /// <returns>The <see cref="BundleManifest"/> object representing the manifest file.</returns>
    /// <remarks>In absence of a stream to the manifest file, this overload uses the virutal path provider to find the manifest file at "~/bundle.config.</remarks>
    public static BundleManifest ReadBundleManifest()
    {
        VirtualPathProvider virtualPathProvider = new VirtualPathProvider(DefaultBundlePath);
        using (var stream = virtualPathProvider.Open())
        {
            return BundleManifest.ReadBundleManifest(stream);
        }
    }

    /// <summary>
    /// Gets the path to the bundle manifest file.
    /// </summary>
    /// <returns>The path to the bundle manifest file.</returns>
    public static string BundleManifestPath
    {
        get
        {
            // TODO: support app settings in the future
            return DefaultBundlePath;
        }
    }

    /*
    /// <summary>
    /// Creates a bundle manifest object from a bundle manifest.
    /// </summary>
    /// <returns>The <see cref="BundleManifest"/> object representing the manifest file.</returns>
    /// <remarks>In absence of a stream to the manifest file, this overload uses the virutal path provider to find the manifest file at "~/bundle.config.</remarks>

    public static BundleManifest ReadBundleManifest()
    {
        return ReadBundleManifest(BundleTable.VirtualPathProvider);
    }

    internal static BundleManifest ReadBundleManifest(VirtualPathProvider vpp)
    {
        if (vpp == null)
        {
            return null;
        }

        if (!vpp.FileExists(BundleManifestPath))
        {
            // If the bundle path is not user-specified and no file exists at the root, don't attempt to set up bundles.
            return null;
        }

        VirtualFile file = vpp.GetFile(BundleManifestPath);
        using (var stream = file.Open())
        {
            return BundleManifest.ReadBundleManifest(stream);
        }
    }*/

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is the recommended pattern.")]
    private static XmlDocument GetXmlDocument(Stream bundleStream)
    {
        var document = new XmlDocument();

        using (var xsdStream = typeof(BundleManifest).Assembly.GetManifestResourceStream(XsdResourceName))
        {
            if (xsdStream is not null)
            {
                using var reader = XmlReader.Create(xsdStream);
                document.Schemas.Add(targetNamespace: null, schemaDocument: reader);
            }
        }

        document.Load(bundleStream);
        document.Validate((sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                // Throw an exception if there is a validation error
                throw new InvalidOperationException(e.Message);
            }
        });

        return document;
    }

    private static BundleDefinition ReadBundle(XmlElement element)
    {
        return new BundleDefinition
        {
            Path = element.GetAttribute("path"),
            CdnPath = element.GetAttribute("cdnPath"),
            CdnFallbackExpression = element.GetAttribute("cdnFallbackExpression"),
            Includes = element.GetElementsByTagName("include").Cast<XmlElement>().Select(s => s.GetAttribute("path")).ToList()
        };
    }

    internal void Register(BundleCollection collection)
    {
        foreach (var bundleData in StyleBundles)
        {
            if (bundleData.Path is not null)
            {
                var styleBundle = new StyleBundle(bundleData.Path);
                styleBundle.Include(bundleData.Includes.ToArray());
                collection.Add(styleBundle);
            }
        }
        foreach (var bundleData in ScriptBundles)
        {
            if (bundleData.Path is not null)
            {
                var styleBundle = new ScriptBundle(bundleData.Path);
                styleBundle.Include(bundleData.Includes.ToArray());
                collection.Add(styleBundle);
            }
        }
    }
}
