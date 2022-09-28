// MIT License.

#nullable disable

namespace System.Web.UI;

// An UrlPropertyAttribute metadata attribute can be applied to string 
// properties that contain URL values.
// This can be used to identify URLs which allows design-time functionality and runtime
// functionality to do interesting things with the property values.
[AttributeUsage(AttributeTargets.Property)]
public sealed class UrlPropertyAttribute : Attribute
{
    // Used to mark a property as an URL.
    public UrlPropertyAttribute() : this("*.*")
    {
    }

    // Used to mark a property as an URL. In addition, the type of files allowed
    // can be specified. This can be used at design-time to customize the URL picker.
    public UrlPropertyAttribute(string filter)
    {
        Filter = filter ?? "*.*";
    }

    // The file filter associated with the URL property. This takes
    // the form of a file filter string typically used with Open File
    // dialogs. The default is *.*, so all file types can be chosen.
    public string Filter { get; }

    public override int GetHashCode()
    {
        return Filter.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }

        UrlPropertyAttribute other = obj as UrlPropertyAttribute;
        return other != null ? Filter.Equals(other.Filter) : false;
    }
}
