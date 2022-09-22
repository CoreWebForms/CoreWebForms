// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI;

public sealed class ValidatorCollection : ICollection
{
    private ArrayList data;

    /// <devdoc>
    /// <para>Initializes a new instance of the <see cref='System.Web.UI.ValidatorCollection'/> class.</para>
    /// </devdoc>
    public ValidatorCollection()
    {
        data = new ArrayList();
    }

    /// <devdoc>
    ///    <para>Indicates the number of references in the collection. 
    ///       This property is read-only.</para>
    /// </devdoc>
    public int Count
    {
        get
        {
            return data.Count;
        }
    }


    /// <devdoc>
    ///    <para>Indicates the validator at the specified index. This 
    ///       property is read-only.</para>
    /// </devdoc>
    public IValidator this[int index]
    {
        get
        {
            return (IValidator)data[index];
        }
    }


    /// <devdoc>
    ///    <para>Adds the specified validator to the collection.</para>
    /// </devdoc>
    public void Add(IValidator validator)
    {
        data.Add(validator);
    }


    /// <devdoc>
    ///    <para>Returns whether the specified validator exists in the collection.</para>
    /// </devdoc>
    public bool Contains(IValidator validator)
    {
        return data.Contains(validator);
    }


    /// <devdoc>
    ///    <para>Removes the specified validator from the collection.</para>
    /// </devdoc>
    public void Remove(IValidator validator)
    {
        data.Remove(validator);
    }


    /// <devdoc>
    ///    <para>Gets an enumerator that iterates over the collection.</para>
    /// </devdoc>
    public IEnumerator GetEnumerator()
    {
        return data.GetEnumerator();
    }



    /// <devdoc>
    ///    <para>Copies a validator to the specified collection and location.</para>
    /// </devdoc>
    public void CopyTo(Array array, int index)
    {
        for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
            array.SetValue(e.Current, index++);
    }


    /// <devdoc>
    ///    <para>Indicates an object that can be used to synchronize the 
    ///    <see cref='System.Web.UI.ValidatorCollection'/> . 
    ///       This property is read-only.</para>
    /// </devdoc>
    public Object SyncRoot
    {
        get { return this; }
    }


    /// <devdoc>
    /// <para>Indicates whether the <see cref='System.Web.UI.ValidatorCollection'/> is read-only. This property is 
    ///    read-only.</para>
    /// </devdoc>
    public bool IsReadOnly
    {
        get { return false; }
    }


    /// <devdoc>
    /// <para>Indicates whether the <see cref='System.Web.UI.ValidatorCollection'/> is synchronized 
    ///    (thread-safe). This property is read-only.</para>
    /// </devdoc>
    public bool IsSynchronized
    {
        get { return false; }
    }
}
