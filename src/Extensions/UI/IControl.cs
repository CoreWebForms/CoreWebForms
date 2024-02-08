// MIT License.

namespace System.Web.UI
{
    using System;

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
