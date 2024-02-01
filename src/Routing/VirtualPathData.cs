// MIT License.

namespace System.Web.Routing;

public class VirtualPathData
{
    private string? _virtualPath;

    public string VirtualPath
    {
        get
        {
            return _virtualPath ?? string.Empty;
        }
        set
        {
            _virtualPath = value;
        }
    }
}
