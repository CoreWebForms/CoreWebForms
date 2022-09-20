// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

/*
 * The StateBag class is a helper class used to manage state of properties.
 * The class stores name/value pairs as string/object and tracks modifications of
 * properties after being 'marked'.  This class is used as the primary storage
 * mechanism for all HtmlControls and WebControls.
 */

/// <devdoc>
///    <para>Manages the state of Web Forms control properties. This 
///       class stores attribute/value pairs as string/object and tracks changes to these
///       attributes, which are treated as properties, after the Page.Init
///       method is executed for a
///       page request. </para>
///    <note type="note">
///       Only values changed after Page.Init
///       has executed are persisted to the page's view state.
///    </note>
/// </devdoc>
public sealed class StateBag : IStateManager, IDictionary
{
    private readonly Dictionary<string, StateItem> bag;

    public StateBag()
        : this(false)
    {
    }
    public StateBag(bool ignoreCase)
    {
        IsTrackingViewState = false;
        bag = new Dictionary<string, StateItem>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    }

    public int Count => bag.Count;

    public ICollection Keys => bag.Keys;

    public ICollection Values => bag.Values;

    /*
     * Get or set value of a StateItem.
     * A set will automatically add a new StateItem for a
     * key which is not already in the bag.  A set to null
     * will remove the item if set before mark, otherwise
     * a null set will be saved to allow tracking of state
     * removed after mark.
     */

    /// <devdoc>
    ///    <para> Indicates the value of an item stored in the 
    ///    <see langword='StateBag'/> 
    ///    object. Setting this property with a key not already stored in the StateBag will
    ///    add an item to the bag. If you set this property to <see langword='null'/> before
    ///    the TrackState method is called on an item will remove it from the bag. Otherwise,
    ///    when you set this property to <see langword='null'/>
    ///    the key will be saved to allow tracking of the item's state.</para>
    /// </devdoc>
    public object? this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
            {
                ArgumentNullException.ThrowIfNull("key");
            }

            if (bag.TryGetValue(key, out var item))
            {
                return item.Value;
            }

            return null;
        }
        set => Add(key, value);
    }

    object? IDictionary.this[object key]
    {
        get => this[(string)key];
        set => this[(string)key] = value;
    }

    [return: NotNullIfNotNull(nameof(value))]
    public StateItem? Add(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        if (bag.TryGetValue(key, out var item))
        {
            if (value == null && !IsTrackingViewState)
            {
                bag.Remove(key);
            }
            else
            {
                item.Value = value;
            }
        }
        else
        {
            if (value != null || IsTrackingViewState)
            {
                item = new StateItem(value);
                bag.Add(key, item);
            }
        }

        if (item is not null && IsTrackingViewState)
        {
            item.IsDirty = true;
        }

        return item;
    }

    void IDictionary.Add(object key, object? value) => Add((string)key, value);

    public void Clear() => bag.Clear();

    public IDictionaryEnumerator GetEnumerator() => new Wrapper(bag.GetEnumerator());

    private class Wrapper : IDictionaryEnumerator
    {
        private readonly IEnumerator<KeyValuePair<string, StateItem>> _inner;

        public Wrapper(IEnumerator<KeyValuePair<string, StateItem>> inner)
        {
            _inner = inner;
        }

        public DictionaryEntry Entry => new(Key, Value);

        public object Key => _inner.Current.Key;

        public object Value => _inner.Current.Value;

        public object Current => Entry;

        public bool MoveNext() => _inner.MoveNext();

        public void Reset() => _inner.Reset();
    }

    public bool IsItemDirty(string key)
    {
        if (bag.TryGetValue(key, out var item))
        {
            return item.IsDirty;
        }

        return false;
    }

    internal bool IsTrackingViewState { get; private set; }

    internal void LoadViewState(object state)
    {
        if (state is null)
        {
            return;
        }

        var typed = (IReadOnlyCollection<KeyValuePair<string, object>>)state;

        foreach (var item in typed)
        {
            Add(item.Key, item.Value);
        }
    }

    internal void TrackViewState() => IsTrackingViewState = true;

    public void Remove(string key) => bag.Remove(key);

    void IDictionary.Remove(object key) => Remove((string)key);

    internal IReadOnlyCollection<KeyValuePair<string, object>> SaveViewState()
    {
        if (bag.Count == 0)
        {
            return Array.Empty<KeyValuePair<string, object>>();
        }

        List<KeyValuePair<string, object>>? list = null;

        foreach (var item in bag)
        {
            if (item.Value.IsDirty && item.Value.Value is { } value)
            {
                (list ??= new()).Add(new(item.Key, value));
            }
        }

        if (list is null)
        {
            return Array.Empty<KeyValuePair<string, object>>();
        }

        return list;
    }

    public void SetDirty(bool dirty)
    {
        if (bag.Count != 0)
        {
            foreach (var item in bag.Values)
            {
                item.IsDirty = dirty;
            }
        }
    }

    public void SetItemDirty(string key, bool dirty)
    {
        if (bag.TryGetValue(key, out var item))
        {
            item.IsDirty = dirty;
        }
    }

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    bool IDictionary.Contains(object key) => bag.ContainsKey((string)key);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection.CopyTo(Array array, int index) => Values.CopyTo(array, index);

    bool IStateManager.IsTrackingViewState => IsTrackingViewState;

    void IStateManager.LoadViewState(object state) => LoadViewState(state);

    void IStateManager.TrackViewState() => TrackViewState();

    object IStateManager.SaveViewState() => SaveViewState();
}
