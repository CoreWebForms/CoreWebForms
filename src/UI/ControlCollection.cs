using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI;

public class ControlCollection : ICollection
{
    private readonly List<Control> _controls = new();
    private readonly Control _owner;

    public ControlCollection(Control owner)
    {
        _owner = owner;
    }

    public void Add(Control control)
    {
        control.Parent = _owner;
        _controls.Add(control);
    }

    public int Count => ((ICollection)_controls).Count;

    public bool IsSynchronized => ((ICollection)_controls).IsSynchronized;

    public object SyncRoot => ((ICollection)_controls).SyncRoot;

    public void CopyTo(Array array, int index)
    {
        ((ICollection)_controls).CopyTo(array, index);
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)_controls).GetEnumerator();
    }
}
