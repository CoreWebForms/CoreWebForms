// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Web.UI.Features;

internal sealed class PageEventsFactory : IPageEventsFactory
{
    private readonly Action<object, EventArgs>? _onLoad;
    private readonly Action<object, EventArgs>? _onPreInit;

    public PageEventsFactory(Type type)
    {
        _onLoad = CreateMethod(type, "Page_Load");
        _onPreInit = CreateMethod(type, "Page_PreInit");
    }

    public IPageEvents Create(Page page) => new PageEvents(page, this);

    private static Action<object, EventArgs>? CreateMethod(Type type, string name)
    {
        var method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method is not null && method.ReturnType == typeof(void))
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                return (object target, EventArgs o) => method.Invoke(target, null);
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventArgs))
            {
                return (object target, EventArgs o) => method.Invoke(target, new object[] { target, o });
            }
        }

        return null;
    }

    private class PageEvents : IPageEvents
    {
        private readonly Page _page;
        private readonly PageEventsFactory _factory;

        public PageEvents(Page page, PageEventsFactory factory)
        {
            _page = page;
            _factory = factory;
        }

        public void OnPageLoad() => _factory.OnPageLoad(_page);

        public void OnPreInit() => _factory.OnPreInit(_page);
    }

    public void OnPageLoad(Page page) => _onLoad?.Invoke(page, EventArgs.Empty);

    public void OnPreInit(Page page) => _onPreInit?.Invoke(page, EventArgs.Empty);
}
