// MIT License.

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Web.UI;
[Serializable]
public sealed class ViewStateException : Exception, ISerializable
{

    // not in System.Web.txt because it should not be localized
    private const string _format = "\r\n\tClient IP: {0}\r\n\tPort: {1}\r\n\tReferer: {2}\r\n\tPath: {3}\r\n\tUser-Agent: {4}\r\n\tViewState: {5}";

    private bool _isConnected = true;
    private string _remoteAddr;
    private string _remotePort;
    private string _userAgent;
    private string _persistedState;
    private string _referer;
    private string _path;
    private string _message;

    internal bool _macValidationError;

    public override string Message { get { return _message; } }
    public string RemoteAddress { get { return _remoteAddr; } }
    public string RemotePort { get { return _remotePort; } }
    public string UserAgent { get { return _userAgent; } }
    public string PersistedState { get { return _persistedState; } }
    public string Referer { get { return _referer; } }
    public string Path { get { return _path; } }
    public bool IsConnected { get { return _isConnected; } }

    [Obsolete]
    private ViewStateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    // Create by calling appropriate Throw*Error method, which wraps the error
    // in an HttpException that displays a meaningful message at the top of the page.

    public ViewStateException() { }

    private ViewStateException(Exception innerException, string persistedState) :
        base(null, innerException)
    {

        Initialize(persistedState);
    }

    private void Initialize(string persistedState)
    {

        _persistedState = persistedState;

        HttpContext context = HttpContext.Current;
        HttpRequest request = context != null ? context.Request : null;
        HttpResponse response = context != null ? context.Response : null;

        _isConnected = response.IsClientConnected;
        _remoteAddr = context.ServerVariable("REMOTE_ADDR", string.Empty);
        _remotePort = context.ServerVariable("REMOTE_PORT", string.Empty);
        _userAgent = context.ServerVariable("HTTP_USER_AGENT", string.Empty);
        _referer = context.ServerVariable("HTTP_REFERER", string.Empty);
        _path = context.ServerVariable("PATH_INFO", string.Empty);

        string debugInfo = String.Format(CultureInfo.InvariantCulture,
                                         _format,
                                         _remoteAddr,
                                         _remotePort,
                                         _referer,
                                         _path,
                                         _userAgent,
                                         _persistedState);

        _message = SR.GetString(SR.ViewState_InvalidViewStatePlus, debugInfo);
    }

    [Obsolete]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }

    internal static string ShortMessage
    {
        get { return SR.ViewState_InvalidViewState; }
    }

    // if the client disconnected, we want to display that at the top of the error page
    private static string GetCorrectErrorPageMessage(ViewStateException vse, string message)
    {
        if (!vse.IsConnected)
        {
            return SR.GetString(SR.ViewState_ClientDisconnected);
        }
        else
        {
            return SR.GetString(message);
        }
    }

    private static void ThrowError(Exception inner, string persistedState, string errorPageMessage,
                                    bool macValidationError)
    {
        ViewStateException middle;
        HttpException outer;

        middle = new ViewStateException(inner, persistedState);
        middle._macValidationError = macValidationError;

        // Setup the formatter for this exception, to make sure this message shows up
        // in an error page as opposed to the inner-most exception's message.
        outer = new HttpException(GetCorrectErrorPageMessage(middle, errorPageMessage), middle);
#if PORT_HTTPEXCEPTION_FORMATTER
        outer.SetFormatter(new UseLastUnhandledErrorFormatter(outer));
#endif

        throw outer;
    }

    internal static void ThrowMacValidationError(Exception inner, string persistedState)
    {
        ThrowError(inner, persistedState, SR.ViewState_AuthenticationFailed, true);
    }

    internal static void ThrowViewStateError(Exception inner, string persistedState)
    {
        ThrowError(inner, persistedState, SR.Invalid_ControlState, false);
    }

    // Returns true if this exception was caused by a view state MAC validation failure; false otherwise
    internal static bool IsMacValidationException(Exception e)
    {
        for (; e != null; e = e.InnerException)
        {
            ViewStateException vse = e as ViewStateException;
            if (vse != null && vse._macValidationError)
            {
                return true;
            }
        }

        // not a ViewState MAC validation exception
        return false;
    }
}
