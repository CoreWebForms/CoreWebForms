// MIT License.

namespace Microsoft.Extensions.Configuration;

internal class WebConfigSource : FileConfigurationSource
{
    public WebConfigSource(string path)
    {
        Path = path;
        ReloadOnChange = true;
        Optional = true;
    }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new WebConfigConfigurationProvider(this);
    }
}
