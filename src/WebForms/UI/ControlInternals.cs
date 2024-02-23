// MIT License.

#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace System.Web.UI;

public partial class Control
{
    protected internal ILogger Logger => Context.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName ?? GetType().Name);
}
