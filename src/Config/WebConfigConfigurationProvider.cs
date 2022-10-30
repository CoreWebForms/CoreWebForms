// MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Extensions.Configuration;

internal sealed class WebConfigConfigurationProvider : FileConfigurationProvider
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

        if (appSettings is not null)
        {
            foreach (XmlNode child in appSettings)
            {
                if (child.Attributes is { } attributes &&
                    attributes["key"] is { Value: { } key } &&
                    attributes["value"] is { Value: { } value })
                {
                    keys.Add(key);
                    data[key] = value;
                }
            }
        }

        return keys;
    }

    private static HashSet<string> ReadConnectionStrings(XmlDocument doc, Dictionary<string, string> data)
    {
        var keys = new HashSet<string>();
        var connectionStrings = doc.SelectNodes("/configuration/connectionStrings/add");

        if (connectionStrings is not null)
        {
            foreach (XmlNode child in connectionStrings)
            {
                if (child.Attributes is { } attributes &&
                    attributes["connectionString"] is { Value: { } connectionString } &&
                    attributes["name"] is { Value: { } name })
                {
                    keys.Add(name);

                    data[$"ConnectionStrings:{name}"] = connectionString;

                    if (attributes["providerName"] is { Value: { } providerName })
                    {
                        data[$"ConnectionStringProviders:{name}"] = providerName;
                    }
                }
            }
        }

        return keys;
    }
}
