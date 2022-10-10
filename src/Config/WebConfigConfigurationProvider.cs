// MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Extensions.Configuration;

internal class WebConfigConfigurationProvider : FileConfigurationProvider
{
    internal KnownKeys Keys { get; private set; } = new(Enumerable.Empty<string>(), Enumerable.Empty<string>());

    public WebConfigConfigurationProvider(FileConfigurationSource source)
        : base(source)
    {
    }

    public override void Load(Stream stream)
    {
        try
        {
            (Data, Keys) = ReadSettings(stream);
        }
        catch (Exception e)
        {
            throw new FormatException("Failed to read from web.config", e);
        }
    }

    private static (Dictionary<string, string>, KnownKeys) ReadSettings(Stream stream)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var doc = new XmlDocument();
        doc.Load(stream);

        var settings = ReadAppSettings(doc, data);
        var strings = ReadConnectionStrings(doc, data);

        return (data, new KnownKeys(settings, strings));
    }

    private static HashSet<string> ReadAppSettings(XmlDocument doc, Dictionary<string, string> data)
    {
        var keys = new HashSet<string>();
        var appSettings = doc.SelectNodes("/configuration/appSettings/add");

        foreach (XmlNode child in appSettings)
        {
            var key = child.Attributes["key"].Value;

            keys.Add(key);
            data[key] = child.Attributes["value"].Value;
        }

        return keys;
    }

    private static new HashSet<string> ReadConnectionStrings(XmlDocument doc, Dictionary<string, string> data)
    {
        var keys = new HashSet<string>();
        var connectionStrings = doc.SelectNodes("/configuration/connectionStrings/add");

        foreach (XmlNode child in connectionStrings)
        {
            var key = child.Attributes["name"].Value;
            var value = child.Attributes["connectionString"].Value;
            var provider = child.Attributes["providerName"].Value;

            keys.Add(key);

            data[$"ConnectionStrings:{key}"] = value;
            data[$"ConnectionStringProviders:{key}"] = provider;
        }

        return keys;
    }
}
