// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

using ViewStateData = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<(string, object)>>;

namespace System.Web.UI.Features;

internal class ViewStateManager : IViewStateManager
{
    private readonly Page _page;
    private readonly IViewStateSerializer _serializer;
    private readonly IDataProtector? _protector;
    private readonly NameValueCollection? _form;

    public ViewStateManager(Page page, HttpContextCore context)
    {
        _page = page;
        _serializer = context.RequestServices.GetRequiredService<IViewStateSerializer>();
        _protector = context.RequestServices.GetService<IDataProtectionProvider>()?.CreateProtector(nameof(ViewStateManager));
        GeneratorId = _page.GetType().GetHashCode().ToString("X8", CultureInfo.InvariantCulture);

        if (context.Request.HasFormContentType)
        {
            if (string.Equals(GeneratorId, context.Request.Form[Page.ViewStateGeneratorFieldID], StringComparison.Ordinal))
            {
                ClientState = context.Request.Form[Page.ViewStateFieldPrefixID];
                OriginalState = ClientState;
                _form = ((HttpContext)context).Request.Form;
            }
        }
    }

    public string ClientState { get; private set; } = string.Empty;

    public string GeneratorId { get; }

    public string OriginalState { get; } = string.Empty;

    public void RefreshControls()
    {
        var data = LoadDictionary(ClientState);

        foreach (var child in _page.AllChildren)
        {
            if (child.ID is { } id)
            {
                if (_form is not null && _form.Get(id) is not null)
                {
                    if (child is IPostBackDataHandler postBack)
                    {
                        postBack.LoadPostData(id, _form);
                    }
                    else if (child is IPostBackEventHandler eventHandler)
                    {
                        eventHandler.RaisePostBackEvent(_form?[Page.postEventArgumentID]);
                    }
                }

                if (data is not null && data.TryGetValue(id, out var values))
                {
                    child.LoadViewStateRecursive(values);
                }
            }
        }
    }

    public void UpdateClientState()
    {
        using var ms = new MemoryStream();

        WriteItems(ms);

        var array = ms.ToArray();

        if (_protector is not null)
        {
            array = _protector.Protect(array);
        }

        ClientState = Convert.ToBase64String(array);
    }

    private ViewStateData? LoadDictionary(string state)
    {
        if (string.IsNullOrEmpty(state))
        {
            return null;
        }

        var data = Convert.FromBase64String(state);

        if (_protector is not null)
        {
            data = _protector.Unprotect(data);
        }

        // Data protection will obfuscate the actual length, so let's check again
        if (data.Length == 0)
        {
            return null;
        }

        using (var ms = new MemoryStream(data))
        using (var stream = new GZipStream(ms, CompressionMode.Decompress))
        using (var reader = new BinaryReader(stream))
        {
            return ReadItems(reader);
        }
    }

    private ViewStateData ReadItems(BinaryReader reader)
    {
        var result = new ViewStateData();

        var controlCount = reader.Read7BitEncodedInt();

        for (int idx = 0; idx < controlCount; idx++)
        {
            var name = reader.ReadString();
            var count = reader.Read7BitEncodedInt();
            var items = new List<(string, object)>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = _serializer.Deserialize(reader);

                if (value is not null)
                {
                    items.Add((key, value));
                }
            }

            result.Add(name, items);
        }

        return result;
    }

    private void WriteItems(Stream stream)
    {
        using var compression = new GZipStream(stream, CompressionLevel.SmallestSize);
        using var writer = new BinaryWriter(compression);

        RecurseControls(writer, _page);
    }

    private void RecurseControls(BinaryWriter writer, Control parent)
    {
        List<ControlState>? _list = null;

        foreach (var control in parent.AllChildren)
        {
            if (control.IsTrackingViewState && control.HasViewState && control.ID is { } id)
            {
                if (control.ViewState.SaveViewState() is { Count: > 0 } state)
                {
                    (_list ??= new()).Add(new(id, state));

                }
            }
        }

        if (_list is not null)
        {
            writer.Write7BitEncodedInt(_list.Count);

            foreach (var control in _list)
            {
                writer.Write(control.Id);
                writer.Write7BitEncodedInt(control.Items.Count);

                foreach (var item in control.Items)
                {
                    writer.Write(item.Key);
                    _serializer.Serialize(writer, item.Value);
                }
            }
        }
    }

    private readonly struct ControlState
    {
        public ControlState(string id, IReadOnlyCollection<KeyValuePair<string, object>> items)
        {
            Id = id;
            Items = items;
        }

        public string Id { get; }

        public IReadOnlyCollection<KeyValuePair<string, object>> Items { get; }
    }
}
