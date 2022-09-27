// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace System.Web.UI;

/// <devdoc>
/// <para>Provides the <see cref='System.Web.UI.Page'/> class and the <see cref='System.Web.UI.UserControl'/> class with a base set of functionality.</para>
/// </devdoc>
public abstract class TemplateControl : Control, INamingContainer
#if PORT_FILTER
    , IFilterResolutionService
#endif
{
    private static readonly object _lockObject = new object();

    // Caches the list of auto-hookup methods for each compiled Type (Hashtable<Type,ListDictionary>).
    // We use a Hashtable instead of the central Cache for optimal performance (VSWhidbey 479476)
    private static readonly Hashtable _eventListCache = new Hashtable();

    private static readonly object _emptyEventSingleton = new EventList();

    private VirtualPath _virtualPath;

#if PORT_RESOURCEPROVIDER
    private IResourceProvider _resourceProvider;
#endif

    private const string _pagePreInitEventName = "Page_PreInit";
    private const string _pageInitEventName = "Page_Init";
    private const string _pageInitCompleteEventName = "Page_InitComplete";
    private const string _pageLoadEventName = "Page_Load";
    private const string _pagePreLoadEventName = "Page_PreLoad";
    private const string _pageLoadCompleteEventName = "Page_LoadComplete";
    private const string _pagePreRenderCompleteEventName = "Page_PreRenderComplete";
    private const string _pagePreRenderCompleteAsyncEventName = "Page_PreRenderCompleteAsync";
    private const string _pageDataBindEventName = "Page_DataBind";
    private const string _pagePreRenderEventName = "Page_PreRender";
    private const string _pageSaveStateCompleteEventName = "Page_SaveStateComplete";
    private const string _pageUnloadEventName = "Page_Unload";
    private const string _pageErrorEventName = "Page_Error";
    private const string _pageAbortTransactionEventName = "Page_AbortTransaction";
    private const string _onTransactionAbortEventName = "OnTransactionAbort";
    private const string _pageCommitTransactionEventName = "Page_CommitTransaction";
    private const string _onTransactionCommitEventName = "OnTransactionCommit";

    private static readonly Hashtable _eventObjects = new Hashtable(16)
    {
        { _pagePreInitEventName, Page.EventPreInit },
        { _pageInitEventName, EventInit },
        { _pageInitCompleteEventName, Page.EventInitComplete },
        { _pageLoadEventName, EventLoad },
        { _pagePreLoadEventName, Page.EventPreLoad },
        { _pageLoadCompleteEventName, Page.EventLoadComplete },
        { _pagePreRenderCompleteEventName, Page.EventPreRenderComplete },
        { _pageDataBindEventName, EventDataBinding },
        { _pagePreRenderEventName, EventPreRender },
        { _pageSaveStateCompleteEventName, Page.EventSaveStateComplete },
        { _pageUnloadEventName, EventUnload },
        { _pageErrorEventName, EventError },
        { _pageAbortTransactionEventName, EventAbortTransaction },
        { _onTransactionAbortEventName, EventAbortTransaction },
        { _pageCommitTransactionEventName, EventCommitTransaction },
        { _onTransactionCommitEventName, EventCommitTransaction }
    };

    protected TemplateControl()
    {
        Construct();
    }

    /// <devdoc>
    /// <para>Do construction time logic (ASURT 66166)</para>
    /// </devdoc>
    protected virtual void Construct() { }

    private static readonly object EventCommitTransaction = new object();

    /// <devdoc>
    ///    <para>Occurs when a user initiates a transaction.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_OnCommitTransaction)
    ]
    public event EventHandler CommitTransaction
    {
        add
        {
            Events.AddHandler(EventCommitTransaction, value);
        }
        remove
        {
            Events.RemoveHandler(EventCommitTransaction, value);
        }
    }

    /// <devdoc>
    ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
    /// </devdoc>
    [
    Browsable(true)
    ]
    public override bool EnableTheming
    {
        get
        {
            return base.EnableTheming;
        }
        set
        {
            base.EnableTheming = value;
        }
    }

    /// <devdoc>
    /// <para>Raises the <see langword='CommitTransaction'/> event. You can use this method
    ///    for any transaction processing logic in which your page or user control
    ///    participates.</para>
    /// </devdoc>
    protected virtual void OnCommitTransaction(EventArgs e)
    {
        EventHandler handler = (EventHandler)Events[EventCommitTransaction];
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private static readonly object EventAbortTransaction = new object();

    /// <devdoc>
    ///    <para>Occurs when a user aborts a transaction.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_OnAbortTransaction)
    ]
    public event EventHandler AbortTransaction
    {
        add
        {
            Events.AddHandler(EventAbortTransaction, value);
        }
        remove
        {
            Events.RemoveHandler(EventAbortTransaction, value);
        }
    }

    /// <devdoc>
    /// <para>Raises the <see langword='AbortTransaction'/> event.</para>
    /// </devdoc>
    protected virtual void OnAbortTransaction(EventArgs e)
    {
        EventHandler handler = (EventHandler)Events[EventAbortTransaction];
        if (handler != null)
        {
            handler(this, e);
        }
    }

    // Page_Error related events/methods

    private static readonly object EventError = new object();

    /// <devdoc>
    ///    <para>Occurs when an uncaught exception is thrown.</para>
    /// </devdoc>
    [
    WebSysDescription(SR.Page_Error)
    ]
    public event EventHandler Error
    {
        add
        {
            Events.AddHandler(EventError, value);
        }
        remove
        {
            Events.RemoveHandler(EventError, value);
        }
    }

    /// <devdoc>
    /// <para>Raises the <see langword='Error'/> event.
    ///    </para>
    /// </devdoc>
    protected virtual void OnError(EventArgs e)
    {
        EventHandler handler = (EventHandler)Events[EventError];
        if (handler != null)
        {
            handler(this, e);
        }
    }

    /*
     * Method sometime overidden by the generated sub classes.  Users
     * should not override.
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>Initializes the requested page. While this is sometimes
    ///       overridden when the page is generated at runtime, you should not explicitly override this method.</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual void FrameworkInitialize()
    {
    }

    /*
     * This property is overriden by the generated classes (hence it cannot be internal)
     * If false, we don't do the HookUpAutomaticHandlers() magic.
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected virtual bool SupportAutoEvents
    {
        get { return true; }
    }

    /*
     * Returns a pointer to the resource buffer, and the largest valid offset
     * in the buffer (for security reason)
     */
    internal IntPtr StringResourcePointer { get; private set; }
    internal int MaxResourceOffset { get; private set; }

    internal VirtualPath VirtualPath
    {
        get
        {
            return _virtualPath;
        }
    }

    [
    EditorBrowsable(EditorBrowsableState.Advanced),
    Browsable(false)
    ]
    public string AppRelativeVirtualPath
    {
        get
        {
            return VirtualPath.GetAppRelativeVirtualPathString(TemplateControlVirtualPath);
        }
        set
        {
            // Set the TemplateSourceDirectory based on the VirtualPath
            this.TemplateControlVirtualPath = VirtualPath.CreateNonRelative(value);
        }
    }

    internal VirtualPath TemplateControlVirtualPath
    {
        get
        {
            return _virtualPath;
        }
        set
        {
            _virtualPath = value;

            // Set the TemplateSourceDirectory based on the VirtualPath
            this.TemplateControlVirtualDirectory = _virtualPath.Parent;
        }
    }

    /// <devdoc>
    ///    <para>Tests if a device filter applies to this request</para>
    /// </devdoc>
    public virtual bool TestDeviceFilter(string filterName)
    {
#if PORT_BROWSER
        return (Context.Request.Browser.IsBrowser(filterName));
#endif
        return false;
    }

    internal override TemplateControl GetTemplateControl()
    {
        return this;
    }

    internal void HookUpAutomaticHandlers()
    {
        // Do nothing if auto-events are not supported
        if (!SupportAutoEvents)
        {
            return;
        }

        // Get the event list for this Type from our cache, if possible
        object o = _eventListCache[GetType()];
        EventList eventList;

        // Try to find what handlers are implemented if not tried before
        if (o == null)
        {
            lock (_lockObject)
            {

                // Try the cache again, in case another thread took care of it
                o = (EventList)_eventListCache[GetType()];

                if (o == null)
                {
                    eventList = new EventList();

                    GetDelegateInformation(eventList);

                    // Cannot find any known handlers.
                    if (eventList.IsEmpty)
                    {
                        o = _emptyEventSingleton;
                    }
                    else
                    {
                        o = eventList;
                    }

                    // Cache it for next time
                    _eventListCache[GetType()] = o;
                }
            }
        }

        // Don't do any thing if no known handlers are found.
        if (o == _emptyEventSingleton)
        {
            return;
        }

        eventList = (EventList)o;
        IDictionary<string, SyncEventMethodInfo> syncEvents = eventList.SyncEvents;

        // Hook up synchronous events
        foreach (var entry in syncEvents)
        {
            string key = entry.Key;
            SyncEventMethodInfo info = entry.Value;

            Debug.Assert(_eventObjects[key] != null);

            bool eventExists = false;
            MethodInfo methodInfo = info.MethodInfo;

            Delegate eventDelegates = Events[_eventObjects[key]];
            if (eventDelegates != null)
            {
                foreach (Delegate eventDelegate in eventDelegates.GetInvocationList())
                {
                    // Ignore if this method is already added to the events list.
                    if (eventDelegate.Method.Equals(methodInfo))
                    {
                        eventExists = true;
                        break;
                    }
                }
            }

#if PORT_EVENTS_POINTER
            if (!eventExists)
            {
                // Create a new Calli delegate proxy
                IntPtr functionPtr = methodInfo.MethodHandle.GetFunctionPointer();
                EventHandler handler = (new CalliEventHandlerDelegateProxy(this, functionPtr, info.IsArgless)).Handler;

                // Adds the delegate to events list.
                Events.AddHandler(_eventObjects[key], handler);
            }
#endif
        }

#if PORT_ASYNCEVENTS
        // Hook up asynchronous events
        IDictionary<string, AsyncEventMethodInfo> asyncEvents = eventList.AsyncEvents;

        AsyncEventMethodInfo preRenderCompleteAsyncEvent;
        if (asyncEvents.TryGetValue(_pagePreRenderCompleteAsyncEventName, out preRenderCompleteAsyncEvent))
        {
            Page page = (Page)this; // this event handler only exists for the Page type
            if (preRenderCompleteAsyncEvent.RequiresCancellationToken)
            {
                var handler = FastDelegateCreator<Func<CancellationToken, Task>>.BindTo(this, preRenderCompleteAsyncEvent.MethodInfo);
                page.RegisterAsyncTask(new PageAsyncTask(handler));
            }
            else
            {
                var handler = FastDelegateCreator<Func<Task>>.BindTo(this, preRenderCompleteAsyncEvent.MethodInfo);
                page.RegisterAsyncTask(new PageAsyncTask(handler));
            }
        }
#endif
    }

    private void GetDelegateInformation(EventList eventList)
    {
        IDictionary<string, SyncEventMethodInfo> syncEventDictionary = eventList.SyncEvents;
        IDictionary<string, AsyncEventMethodInfo> asyncEventDictionary = eventList.AsyncEvents;

        if (this is Page)
        {
            /* SYNCHRONOUS - Page */

            GetDelegateInformationFromSyncMethod(_pagePreInitEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pagePreLoadEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageLoadCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pagePreRenderCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageInitCompleteEventName, syncEventDictionary);
            GetDelegateInformationFromSyncMethod(_pageSaveStateCompleteEventName, syncEventDictionary);

            /* ASYNCHRONOUS - Page */

            GetDelegateInformationFromAsyncMethod(_pagePreRenderCompleteAsyncEventName, asyncEventDictionary);
        }

        /* SYNCHRONOUS - Control */

        GetDelegateInformationFromSyncMethod(_pageInitEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageLoadEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageDataBindEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pagePreRenderEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageUnloadEventName, syncEventDictionary);
        GetDelegateInformationFromSyncMethod(_pageErrorEventName, syncEventDictionary);

        if (!GetDelegateInformationFromSyncMethod(_pageAbortTransactionEventName, syncEventDictionary))
        {
            GetDelegateInformationFromSyncMethod(_onTransactionAbortEventName, syncEventDictionary);
        }

        if (!GetDelegateInformationFromSyncMethod(_pageCommitTransactionEventName, syncEventDictionary))
        {
            GetDelegateInformationFromSyncMethod(_onTransactionCommitEventName, syncEventDictionary);
        }

        /* ASYNCHRONOUS - Control */

    }

    private bool GetDelegateInformationFromAsyncMethod(string methodName, IDictionary<string, AsyncEventMethodInfo> dictionary)
    {
        // First, try to get a delegate to the single-parameter handler
        MethodInfo parameterfulMethod = GetInstanceMethodInfo(typeof(Func<CancellationToken, Task>), methodName);
        if (parameterfulMethod != null)
        {
            dictionary[methodName] = new AsyncEventMethodInfo(parameterfulMethod, requiresCancellationToken: true);
            return true;
        }

        // If there isn't one, try the argless one
        MethodInfo parameterlessMethod = GetInstanceMethodInfo(typeof(Func<Task>), methodName);
        if (parameterlessMethod != null)
        {
            dictionary[methodName] = new AsyncEventMethodInfo(parameterlessMethod, requiresCancellationToken: false);
            return true;
        }

        return false;
    }

    private bool GetDelegateInformationFromSyncMethod(string methodName, IDictionary<string, SyncEventMethodInfo> dictionary)
    {
        // First, try to get a delegate to the two parameter handler
        MethodInfo parameterfulMethod = GetInstanceMethodInfo(typeof(EventHandler), methodName);
        if (parameterfulMethod != null)
        {
            dictionary[methodName] = new SyncEventMethodInfo(parameterfulMethod, isArgless: false);
            return true;
        }

        // If there isn't one, try the argless one
        MethodInfo parameterlessMethod = GetInstanceMethodInfo(typeof(Action), methodName);
        if (parameterlessMethod != null)
        {
            dictionary[methodName] = new SyncEventMethodInfo(parameterlessMethod, isArgless: true);
            return true;
        }

        return false;
    }

    private MethodInfo GetInstanceMethodInfo(Type delegateType, string methodName)
    {
        Delegate del = Delegate.CreateDelegate(
            type: delegateType,
            target: this,
            method: methodName,
            ignoreCase: true,
            throwOnBindFailure: false);

        return (del != null) ? del.Method : null;
    }

#if PORT_EVAL
    /// <devdoc>
    /// Simplified databinding Eval() method. This method uses the current data item to evaluate an expression using DataBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object Eval(string expression)
    {
        CheckPageExists();
        return DataBinder.Eval(Page.GetDataItem(), expression);
    }

    /// <devdoc>
    /// Simplified databinding Eval() method with a format expression. This method uses the current data item to evaluate an expression using DataBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string Eval(string expression, string format)
    {
        CheckPageExists();
        return DataBinder.Eval(Page.GetDataItem(), expression, format);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method. This method uses the current data item to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object XPath(string xPathExpression)
    {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal object XPath(string xPathExpression, IXmlNamespaceResolver resolver)
    {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, resolver);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method with a format expression. This method uses the current data item to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string XPath(string xPathExpression, string format)
    {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, format);
    }

    /// <devdoc>
    /// Simplified databinding XPath() method with a format expression. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression using XPathBinder.Eval().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver)
    {
        CheckPageExists();
        return XPathBinder.Eval(Page.GetDataItem(), xPathExpression, format, resolver);
    }

    /// <devdoc>
    /// Simplified databinding XPathSelect() method. This method uses the current data item to evaluate an XPath expression that returns a node list using XPathBinder.Select().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal IEnumerable XPathSelect(string xPathExpression)
    {
        CheckPageExists();
        return XPathBinder.Select(Page.GetDataItem(), xPathExpression);
    }

    /// <devdoc>
    /// Simplified databinding XPathSelect() method. This method uses the current data item and a namespace resolver
    /// to evaluate an XPath expression that returns a node list using XPathBinder.Select().
    /// The data item is retrieved using either the IDataItemContainer interface or by looking for a property called 'DataItem'.
    /// If the data item is not found, an exception is thrown.
    /// </devdoc>
    protected internal IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver)
    {
        CheckPageExists();
        return XPathBinder.Select(Page.GetDataItem(), xPathExpression, resolver);
    }
#endif

    private class EventList
    {
        internal readonly IDictionary<string, AsyncEventMethodInfo> AsyncEvents = new Dictionary<string, AsyncEventMethodInfo>(StringComparer.Ordinal);
        internal readonly IDictionary<string, SyncEventMethodInfo> SyncEvents = new Dictionary<string, SyncEventMethodInfo>(StringComparer.Ordinal);

        internal bool IsEmpty
        {
            get
            {
                return (AsyncEvents.Count == 0 && SyncEvents.Count == 0);
            }
        }
    }

    // Internal helper class for storing the event info
    private class SyncEventMethodInfo
    {
        internal SyncEventMethodInfo(MethodInfo methodInfo, bool isArgless)
        {
#if PORT_ASYNC
            if (IsAsyncVoidMethod(methodInfo))
            {
                SynchronizationContextUtil.ValidateModeForPageAsyncVoidMethods();
            }
#endif

            MethodInfo = methodInfo;
            IsArgless = isArgless;
        }

        internal bool IsArgless { get; private set; }
        internal MethodInfo MethodInfo { get; private set; }
    }

    private class AsyncEventMethodInfo
    {
        internal AsyncEventMethodInfo(MethodInfo methodInfo, bool requiresCancellationToken)
        {
            MethodInfo = methodInfo;
            RequiresCancellationToken = requiresCancellationToken;
        }

        internal MethodInfo MethodInfo { get; private set; }
        internal bool RequiresCancellationToken { get; private set; }
    }
}
