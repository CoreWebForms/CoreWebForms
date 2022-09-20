// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Web.UI.WebControls;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.UI.Features;

internal class ViewStateManager : IViewStateManager
{
    private readonly Page _page;
    private readonly IViewStateSerializer _serializer;
    private readonly NameValueCollection _form;

    public ViewStateManager(Page page, HttpContextCore context)
    {
        _page = page;
        _serializer = context.RequestServices.GetRequiredService<IViewStateSerializer>();
        GeneratorId = _page.GetType().GetHashCode().ToString("X8", CultureInfo.InvariantCulture);

        if (context.Request.HasFormContentType)
        {
            if (string.Equals(GeneratorId, context.Request.Form[Page.ViewStateGeneratorFieldID], StringComparison.Ordinal))
            {
                ClientState = context.Request.Form[Page.ViewStateFieldPrefixID];
                _form = ((HttpContext)context).Request.Form;
            }
        }
    }

    public string ClientState { get; private set; } = string.Empty;

    public string GeneratorId { get; }

    public void RefreshControls()
    {
        if (LoadDictionary(ClientState) is { } data)
        {
        }

        ProcessPostbacks();
    }

    private void ProcessPostbacks()
    {
        if (_form is null)
        {
            return;
        }

        foreach (var child in _page.AllChildren)
        {
            if (child is IPostBackDataHandler postBack && child.ID is { } id && _form.Get(id) is not null)
            {
                postBack.LoadPostData(id, _form);
            }
        }
    }

    public void UpdateClientState()
    {
        using var ms = new MemoryStream();

        WriteItems(ms);

        ClientState = Convert.ToBase64String(ms.ToArray());
    }

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        UnknownTypeHandling = Text.Json.Serialization.JsonUnknownTypeHandling.JsonElement
    };

    private Dictionary<string, List<(string, object)>>? LoadDictionary(string state)
    {
        if (string.IsNullOrEmpty(state))
        {
            return null;
        }

        if (_form is null || _form.Count == 0)
        {
            return null;
        }

        var result = GetFromState(state);


        return result;
    }

    private Dictionary<string, List<(string, object)>> GetFromState(string state)
    {
        using (var ms = new MemoryStream(Convert.FromBase64String(state)))
        using (var stream = new GZipStream(ms, CompressionMode.Decompress))
        using (var reader = new BinaryReader(stream))
        {
            return ReadItems(reader);
        }
    }

    private Dictionary<string, List<(string, object)>> ReadItems(BinaryReader reader)
    {
        var result = new Dictionary<string, List<(string, object)>>();

        try
        {
            while (true)
            {
                var name = reader.ReadString();
                var count = reader.Read7BitEncodedInt();
                var items = new List<(string, object)>(count);

                for (int i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = _serializer.Deserialize(reader);
                    items.Add((key, value));
                }

                result.Add(name, items);
            }
        }
        catch (EndOfStreamException)
        {
        }

        return result;
    }

    private void WriteItems(Stream stream)
    {
        using var compression = new GZipStream(stream, CompressionLevel.SmallestSize);
        using var writer = new BinaryWriter(compression);

        RecurseControls(writer, _page);
    }

    private void RecurseControls(BinaryWriter writer, Control control)
    {
        if (control.IsTrackingViewState && control.HasViewState && control.ID is { } id)
        {
            if (control.ViewState.SaveViewState() is { Count: > 0 } state)
            {
                writer.Write(id);
                writer.Write7BitEncodedInt(state.Count);

                foreach (var item in state)
                {
                    writer.Write(item.Key);
                    _serializer.Serialize(writer, item.Value);
                }
            }
        }

        foreach (var child in control.Controls.OfType<Control>())
        {
            RecurseControls(writer, child);
        }
    }

    private class ControlState
    {
        public string Id { get; set; } = null!;

        public List<Tuple<string, object>> Items { get; set; } = null!;
    }
}
