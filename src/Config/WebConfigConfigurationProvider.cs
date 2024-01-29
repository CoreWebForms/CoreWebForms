// MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.Extensions.Configuration;

internal sealed class WebConfigConfigurationProvider : FileConfigurationProvider
{
    public WebConfigConfigurationProvider(FileConfigurationSource source)
        : base(source)
    {
    }

    public override void Load(Stream stream)
    {
        try
        {
            Data = ReadSettings(stream);
        }
        catch (Exception e)
        {
            throw new FormatException("Failed to read from web.config", e);
        }
    }

    private static Dictionary<string, string?> ReadSettings(Stream stream)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var doc = new XmlDocument();
        doc.Load(stream);

        ReadAppSettings(doc, data);
        ReadConnectionStrings(doc, data);

        return data;
    }

    private static void ReadAppSettings(XmlDocument doc, Dictionary<string, string?> data)
    {
        var appSettings = doc.SelectNodes("/configuration/appSettings/add");

        if (appSettings is not null)
        {
            foreach (XmlNode child in appSettings)
            {
                if (child.Attributes is { } attributes &&
                    attributes["key"] is { Value: { } key } &&
                    attributes["value"] is { Value: { } value })
                {
                    data[key] = value;
                }
            }
        }
    }

    private static void ReadConnectionStrings(XmlDocument doc, Dictionary<string, string?> data)
    {
        var connectionStrings = doc.SelectNodes("/configuration/connectionStrings/add");

        if (connectionStrings is not null)
        {
            foreach (XmlNode child in connectionStrings)
            {
                if (child.Attributes is { } attributes &&
                    attributes["connectionString"] is { Value: { } connectionString } &&
                    attributes["name"] is { Value: { } name })
                {
                    data[$"ConnectionStrings:{name}"] = connectionString;

                    if (attributes["providerName"] is { Value: { } providerName })
                    {
                        data[$"ConnectionStringProviders:{name}"] = providerName;
                    }
                }
            }
        }
    }
}
