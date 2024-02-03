// MIT License.

using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public interface IWebFormsBuilder
{
    IServiceCollection Services { get; }

    ISystemWebAdapterBuilder SystemWebAdapterBuilder { get; }
}
