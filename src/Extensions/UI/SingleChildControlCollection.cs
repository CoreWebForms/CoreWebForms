// MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Web.Resources;

namespace System.Web.UI;

public sealed class SingleChildControlCollection : ControlCollection
{
    private bool _allowClear;

    public SingleChildControlCollection(Control owner)
        : base(owner)
    {
    }

    public void AddSingleChild(Control child)
    {
        Debug.Assert(Count == 0, "The collection must be empty if this is called");
        base.Add(child);
    }

    public override void Add(Control child)
    {
        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
            AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
    }

    public override void AddAt(int index, Control child)
    {
        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
            AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
    }

    public override void Clear()
    {
        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
            AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
    }

    public void ClearInternal()
    {
        try
        {
            _allowClear = true;
            base.Clear();
        }
        finally
        {
            _allowClear = false;
        }
    }

    public override void Remove(Control value)
    {
        if (!_allowClear)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
        }

        base.Remove(value);
    }

    public override void RemoveAt(int index)
    {
        if (!_allowClear)
        {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
        }

        base.RemoveAt(index);
    }
}
