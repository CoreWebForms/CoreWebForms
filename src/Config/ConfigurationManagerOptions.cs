// MIT License.

using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration;

public class ConfigurationManagerOptions
{
    public bool HandleReload { get; set; }

    public ICollection<string> KnownAppSettings { get; } = new HashSet<string>();

    public ICollection<string> KnownConnectionStrings { get; } = new HashSet<string>();
}
