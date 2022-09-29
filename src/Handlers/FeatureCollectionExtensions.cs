// MIT License.

using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class FeatureCollectionExtensions
{
    public static T GetRequired<T>(this IFeatureCollection features)
        => features.Get<T>() ?? throw new InvalidOperationException($"No feature of type {features.GetType()} is available.");
}
