// MIT License.

using System.Web.UI;

namespace System.Web.Compilation;
public sealed class ExpressionBuilderContext
{

    // 

    private readonly TemplateControl _templateControl;
    private readonly VirtualPath _virtualPath;

    internal ExpressionBuilderContext(VirtualPath virtualPath)
    {
        _virtualPath = virtualPath;
    }

    public ExpressionBuilderContext(string virtualPath)
    {
        _virtualPath = System.Web.VirtualPath.Create(virtualPath);
    }

    public ExpressionBuilderContext(TemplateControl templateControl)
    {
        _templateControl = templateControl;
    }

    public TemplateControl TemplateControl
    {
        get
        {
            return _templateControl;
        }
    }

    public string VirtualPath
    {
        get
        {
            if (_virtualPath == null && _templateControl != null)
            {
                return _templateControl.AppRelativeVirtualPath;
            }

            return System.Web.VirtualPath.GetVirtualPathString(_virtualPath);
        }
    }

    internal VirtualPath VirtualPathObject
    {
        get
        {
            if (_virtualPath == null && _templateControl != null)
            {
                return _templateControl.VirtualPath;
            }

            return _virtualPath;
        }
    }

}

