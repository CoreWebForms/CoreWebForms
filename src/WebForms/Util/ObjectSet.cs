// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Util;
internal class ObjectSet : ICollection
{

    private class EmptyEnumerator : IEnumerator
    {
        public object Current { get { return null; } }
        public bool MoveNext() { return false; }
        public void Reset() { }
    }

    private static EmptyEnumerator _emptyEnumerator = new EmptyEnumerator();
    private IDictionary _objects;

    internal ObjectSet() { }

    // By default, it's case sensitive
    protected virtual bool CaseInsensitive { get { return false; } }

    public void Add(object o)
    {
        if (_objects == null)
            _objects = new System.Collections.Specialized.HybridDictionary(CaseInsensitive);

        _objects[o] = null;
    }

    public void AddCollection(ICollection c)
    {
        foreach (object o in c)
        {
            Add(o);
        }
    }

    public void Remove(object o)
    {
        if (_objects == null)
            return;

        _objects.Remove(o);
    }

    public bool Contains(object o)
    {
        if (_objects == null)
            return false;

        return _objects.Contains(o);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (_objects == null)
            return _emptyEnumerator;

        return _objects.Keys.GetEnumerator();
    }

    public int Count
    {
        get
        {
            if (_objects == null)
                return 0;
            return _objects.Keys.Count;
        }
    }

    bool ICollection.IsSynchronized
    {
        get
        {
            if (_objects == null)
                return true;
            return _objects.Keys.IsSynchronized;
        }
    }

    object ICollection.SyncRoot
    {
        get
        {
            if (_objects == null)
                return this;
            return _objects.Keys.SyncRoot;
        }
    }

    public void CopyTo(Array array, int index)
    {
        if (_objects != null)
            _objects.Keys.CopyTo(array, index);
    }
}

internal class StringSet : ObjectSet
{
    internal StringSet() { }
}

internal class CaseInsensitiveStringSet : StringSet
{
    protected override bool CaseInsensitive { get { return true; } }
}

internal class VirtualPathSet : ObjectSet
{
    internal VirtualPathSet() { }
}

internal class AssemblySet : ObjectSet
{
    internal AssemblySet() { }

    internal static AssemblySet Create(ICollection c)
    {
        AssemblySet objectSet = new AssemblySet();
        objectSet.AddCollection(c);
        return objectSet;
    }
}

internal class BuildProviderSet : ObjectSet
{
    internal BuildProviderSet() { }
}

internal class ControlSet : ObjectSet
{
    internal ControlSet() { }
}
 
 
