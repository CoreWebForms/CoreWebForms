// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.UI.Features;

internal interface IViewStateManager
{
    string GeneratorId { get; }

    string ClientState { get; }

    void UpdateClientState();

    void RefreshControls();
}

internal interface IViewStateSerializer
{
    void Serialize(BinaryWriter writer, object obj);

    object? Deserialize(BinaryReader reader);
}

internal class ViewStateSerializer : IViewStateSerializer
{
    private readonly Dictionary<Type, int> _fromType = new();
    private readonly List<TypeConverter> _list = new();

    public object? Deserialize(BinaryReader reader)
    {
        var converter = GetConverter(reader.Read7BitEncodedInt());

        return converter.ConvertFromInvariantString(reader.ReadString());
    }

    public void Serialize(BinaryWriter writer, object obj)
    {
        var (converter, index) = GetConverter(obj.GetType());

        writer.Write7BitEncodedInt(index);
        writer.Write(converter.ConvertToInvariantString(obj) ?? string.Empty);
    }

    private (TypeConverter, int) GetConverter(Type type)
    {
        lock (_fromType)
        {
            if (_fromType.TryGetValue(type, out var result))
            {
                return (_list[result], result);
            }

            var converter = TypeDescriptor.GetConverter(type);
            var index = _list.Count;
            _list.Add(converter);
            _fromType.Add(type, index);
            return (converter, index);
        }
    }

    private TypeConverter GetConverter(int index)
    {
        lock (_fromType)
        {
            if (index < _list.Count)
            {
                return _list[index];
            }

            throw new InvalidOperationException("Unknown view state type");
        }
    }
}

internal class ViewStateManager : IViewStateManager
{
    private readonly Page _page;
    private readonly IViewStateSerializer _serializer;
    private readonly IFormCollection? _form;

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
                _form = context.Request.Form;
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

        if (_form is not null)
        {
            foreach (var item in _form)
            {
                if (item.Key.StartsWith(Page.systemPostFieldPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!result.TryGetValue(item.Key, out var existing))
                {
                    existing = new();
                    result.Add(item.Key, existing);
                }

                existing.Add(("Value", item.Value));
            }
        }

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
