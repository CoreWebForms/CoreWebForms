// MIT License.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.AspNetCore.SystemWebAdapters.Compiler.ParserImpl;

public struct TagAttributes : IEnumerable<KeyValuePair<string, string>>
{
    public static TagAttributes Empty { get; } = new TagAttributes(null, false, ImmutableDictionary<string, string>.Empty);

    private readonly ImmutableDictionary<string, string> table;

    public TagAttributes(string id, bool isRunAtServer, ImmutableDictionary<string, string> table)
    {
        Id = id;
        IsRunAtServer = isRunAtServer;
        this.table = table;
    }

    public string Id { get; }

    public bool IsRunAtServer { get; }

    public string this[string key]
    {
        get
        {
            string value;
            return table.TryGetValue(key, out value)
                ? value
                : null;
        }
    }

    public bool ContainsKey(string key) =>
        table.ContainsKey(key);

    public bool TryGetValue(string key, out string value) =>
        table.TryGetValue(key, out value);

    public ImmutableDictionary<string, string>.Enumerator GetEnumerator() =>
        table.GetEnumerator();

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() =>
        GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
