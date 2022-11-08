// MIT License.

namespace System.Web.Routing;

internal class RoutingAdapterFeature : Microsoft.AspNetCore.Routing.IRoutingFeature, IRoutingAdapterFeature
{
    private readonly Microsoft.AspNetCore.Routing.IRoutingFeature _other;

    private System.Web.Routing.RouteData? _data;

    public RoutingAdapterFeature(Microsoft.AspNetCore.Routing.IRoutingFeature other)
    {
        _other = other;
    }

    Microsoft.AspNetCore.Routing.RouteData? Microsoft.AspNetCore.Routing.IRoutingFeature.RouteData
    {
        get => _other.RouteData;
        set
        {
            _other.RouteData = value;
            _data = null;
        }
    }

    public System.Web.Routing.RouteData? RouteData
    {
        get
        {
            if (_data is null)
            {
            }

            if (_other.RouteData is { } data)
            {
                _data = new(data);
            }

            return _data;
        }
    }
}
