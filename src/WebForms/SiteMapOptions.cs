// MIT License.

using System.Configuration;

namespace System.Web;

public class SiteMapOptions
{
    public string DefaultProvider { get; set; }
    public bool? Enabled { get; set; }
    public ICollection<ProviderSettings> Providers { get; } = [];
}
