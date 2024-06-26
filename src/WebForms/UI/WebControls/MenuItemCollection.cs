//MIT license

using System.Collections;
using System.Globalization;
using System.Text;

namespace System.Web.UI.WebControls;

public sealed class MenuItemCollection : ICollection, IStateManager {
    private readonly List<MenuItem> _list;
    private readonly MenuItem _owner;
    private int _version;

    private bool _isTrackingViewState;

    private List<LogItem> _log;

    public MenuItemCollection() : this(null) {
    }

    public MenuItemCollection(MenuItem owner) {
        _owner = owner;
        _list = new List<MenuItem>();
    }

    public int Count => _list.Count;

    public bool IsSynchronized => ((ICollection)_list).IsSynchronized;

    private List<LogItem> Log {
        get {
            if (_log == null) {
                _log = new List<LogItem>();
            }
            return _log;
        }
    }

    public object SyncRoot => ((ICollection)_list).SyncRoot;

    public MenuItem this[int index] {
        get {
            return _list[index];
        }
    }

    public void Add(MenuItem child) {
        AddAt(_list.Count, child);
    }

    public void AddAt(int index, MenuItem child) {
        if (child == null) {
            throw new ArgumentNullException(nameof(child));
        }

        if (child.Owner != null && child.Parent == null) {
            child.Owner.Items.Remove(child);
        }
        if (child.Parent != null) {
            child.Parent.ChildItems.Remove(child);
        }

        if (_owner != null) {
            child.SetParent(_owner);
            child.SetOwner(_owner.Owner);
        }

        _list.Insert(index, child);
        _version++;

        if (_isTrackingViewState) {
            ((IStateManager)child).TrackViewState();
            child.SetDirty();
        }
        Log.Add(new LogItem(LogItemType.Insert, index, _isTrackingViewState));
    }

    public void Clear() {
        if (this.Count == 0) return;
        if (_owner != null) {
            Menu owner = _owner.Owner;
            if (owner != null) {
                MenuItem current = owner.SelectedItem;
                // Check if the selected item is under this collection
                while (current != null) {
                    if (this.Contains(current)) {
                        owner.SetSelectedItem(null);
                        break;
                    }
                    current = current.Parent;
                }
            }
        }
        foreach (MenuItem item in _list) {
            item.SetParent(null);
        }
        _list.Clear();
        _version++;
        if (_isTrackingViewState) {
            // Clearing invalidates all previous log entries, so we can just clear them out and save some space
            Log.Clear();
        }
        Log.Add(new LogItem(LogItemType.Clear, 0, _isTrackingViewState));
    }

    public void CopyTo(Array array, int index) {
        if (!(array is MenuItem[])) {
            throw new ArgumentException(SR.GetString(SR.MenuItemCollection_InvalidArrayType), nameof(array));
        }
        _list.CopyTo((MenuItem[])array, index);
    }

    public void CopyTo(MenuItem[] array, int index) {
        _list.CopyTo(array, index);
    }

    public bool Contains(MenuItem c) {
        return _list.Contains(c);
    }

    internal MenuItem FindItem(string[] path, int pos) {
        if (pos == path.Length) {
            return _owner;
        }

        string pathPart = TreeView.UnEscape(path[pos]);
        for (int i = 0; i < Count; i++) {
            MenuItem node = _list[i];
            if (node.Value == pathPart) {
                return node.ChildItems.FindItem(path, pos + 1);
            }
        }

        return null;
    }

    public IEnumerator GetEnumerator() {
        return new MenuItemCollectionEnumerator(this);
    }

    public int IndexOf(MenuItem value) {
        return _list.IndexOf(value);
    }

    public void Remove(MenuItem value) {
        if (value == null) {
            throw new ArgumentNullException(nameof(value));
        }

        int index = _list.IndexOf(value);
        if (index != -1) {
            RemoveAt(index);
        }
    }

    public void RemoveAt(int index) {
        MenuItem item = _list[index];
        Menu owner = item.Owner;
        if (owner != null) {
            MenuItem current = owner.SelectedItem;
            // Check if the selected item is under this item
            while (current != null) {
                if (current == item) {
                    owner.SetSelectedItem(null);
                    break;
                }
                current = current.Parent;
            }
        }
        item.SetParent(null);

        _list.RemoveAt(index);
        _version++;
        Log.Add(new LogItem(LogItemType.Remove, index, _isTrackingViewState));
    }

    internal void SetDirty() {
        foreach (LogItem item in Log) {
            item.Tracked = true;
        }
        for (int i = 0; i < Count; i++) {
            this[i].SetDirty();
        }
    }

    #region IStateManager implementation

    /// <internalonly/>
    bool IStateManager.IsTrackingViewState => _isTrackingViewState;

    /// <internalonly/>
    void IStateManager.LoadViewState(object state) {
        object[] nodeState = (object[])state;
        if (nodeState != null) {
            if (nodeState[0] != null) {
                string logString = (string)nodeState[0];
                // Process each log entry
                string[] items = logString.Split(',');
                for (int i = 0; i < items.Length; i++) {
                    string[] parts = items[i].Split(':');
                    LogItemType type = (LogItemType)Int32.Parse(parts[0], CultureInfo.InvariantCulture);
                    int index = Int32.Parse(parts[1], CultureInfo.InvariantCulture);

                    if (type == LogItemType.Insert) {
                        AddAt(index, new MenuItem());
                    }
                    else if (type == LogItemType.Remove) {
                        RemoveAt(index);
                    }
                    else if (type == LogItemType.Clear) {
                        Clear();
                    }
                }
            }

            for (int i = 0; i < nodeState.Length - 1; i++) {
                if ((nodeState[i + 1] != null) && (this[i] != null)) {
                    ((IStateManager)this[i]).LoadViewState(nodeState[i + 1]);
                }
            }
        }
    }

    /// <internalonly/>
    object IStateManager.SaveViewState() {
        object[] nodes = new object[Count + 1];

        bool hasViewState = false;

        if ((_log != null) && (_log.Count > 0)) {
            // Construct a string representation of the log, delimiting entries with commas
            // and seperator command and index with a colon
            StringBuilder builder = new StringBuilder();
            int realLogCount = 0;
            for (int i = 0; i < _log.Count; i++) {
                LogItem item = (LogItem)_log[i];
                if (item.Tracked) {
                    builder.Append((int)item.Type);
                    builder.Append(':');
                    builder.Append(item.Index);
                    if (i < (_log.Count - 1)) {
                        builder.Append(',');
                    }

                    realLogCount++;
                }
            }

            if (realLogCount > 0) {
                nodes[0] = builder.ToString();
                hasViewState = true;
            }
        }

        for (int i = 0; i < Count; i++) {
            nodes[i + 1] = ((IStateManager)this[i]).SaveViewState();
            if (nodes[i + 1] != null) {
                hasViewState = true;
            }
        }

        return (hasViewState ? nodes : null);
    }

    /// <internalonly/>
    void IStateManager.TrackViewState() {
        _isTrackingViewState = true;
        for (int i = 0; i < Count; i++) {
            ((IStateManager)this[i]).TrackViewState();
        }
    }
    #endregion

    /// <devdoc>
    ///     Convenience class for storing and using log entries.
    /// </devdoc>
    private class LogItem {
        private readonly LogItemType _type;
        private readonly int _index;
        private bool _tracked;

        public LogItem(LogItemType type, int index, bool tracked) {
            _type = type;
            _index = index;
            _tracked = tracked;
        }

        public int Index => _index;

        public bool Tracked {
            get {
                return _tracked;
            }
            set {
                _tracked = value;
            }
        }

        public LogItemType Type => _type;

    }

    /// <devdoc>
    ///     Convenience enumeration for identifying log commands
    /// </devdoc>
    private enum LogItemType {
        Insert = 0,
        Remove = 1,
        Clear = 2
    }

    // This is a copy of the ArrayListEnumeratorSimple in ArrayList.cs
    private class MenuItemCollectionEnumerator : IEnumerator {
        private readonly MenuItemCollection list;
        private int index;
        private readonly int version;
        private MenuItem currentElement;

        internal MenuItemCollectionEnumerator(MenuItemCollection list) {
            this.list = list;
            this.index = -1;
            version = list._version;
        }

        public bool MoveNext() {
            if (version != list._version)
                throw new InvalidOperationException(SR.GetString(SR.ListEnumVersionMismatch));

            if (index < (list.Count - 1)) {
                index++;
                currentElement = list[index];
                return true;
            }
            else
                index = list.Count;
            return false;
        }

        object IEnumerator.Current => Current;

        public MenuItem Current {
            get {
                if (index == -1)
                    throw new InvalidOperationException(SR.GetString(SR.ListEnumCurrentOutOfRange));
                if (index >= list.Count)
                    throw new InvalidOperationException(SR.GetString(SR.ListEnumCurrentOutOfRange));
                return currentElement;
            }
        }

        public void Reset() {
            if (version != list._version)
                throw new InvalidOperationException(SR.GetString(SR.ListEnumVersionMismatch));
            currentElement = null;
            index = -1;
        }
    }
}
