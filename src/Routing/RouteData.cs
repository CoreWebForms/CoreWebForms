// MIT License.

namespace System.Web.Routing;

public class RouteData
{
    private readonly Microsoft.AspNetCore.Routing.RouteData _data;

    private RouteValueDictionary? _tokens;
    private RouteValueDictionary? _values;

    internal RouteData(Microsoft.AspNetCore.Routing.RouteData data)
    {
        _data = data;
    }

    internal Microsoft.AspNetCore.Routing.RouteData AsAspNetCore() => _data;

    public RouteValueDictionary DataTokens => _tokens ??= new(_data.DataTokens);

    public RouteValueDictionary Values => _values ??= new(_data.Values);

    public string GetRequiredString(string valueName)
    {
        if (Values.TryGetValue(valueName, out var value))
        {
            var valueString = value as string;
            if (!string.IsNullOrEmpty(valueString))
            {
                return valueString;
            }
        }

        throw new InvalidOperationException($"RouteData requires value '{valueName}'");
    }
}
