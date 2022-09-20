// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Web.UI.Features;

internal sealed class PageEvents : IPageEvents
{
    private readonly Action<object, EventArgs>? _onLoad;

    public PageEvents(Type type)
    {
        var method = type.GetMethod("Page_Load", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method is not null && method.ReturnType == typeof(void))
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                _onLoad = (object target, EventArgs o) => method.Invoke(target, null);
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventArgs))
            {
                _onLoad = (object target, EventArgs o) => method.Invoke(target, new object[] { target, o });
            }
        }
    }

    public void OnPageLoad(Page page)
    {
        _onLoad?.Invoke(page, EventArgs.Empty);
    }
}
