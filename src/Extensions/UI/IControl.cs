// MIT License.

namespace System.Web.UI
{
    internal interface IControl : IClientUrlResolver
    {
        HttpContextBase Context
        {
            get;
        }
        bool DesignMode
        {
            get;
        }
    }
}
