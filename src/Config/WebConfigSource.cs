// MIT License.

namespace Microsoft.Extensions.Configuration;

internal sealed class WebConfigSource : FileConfigurationSource
{
    public WebConfigSource(string path, bool isOptional)
    {
        Path = path;
        ReloadOnChange = true;
        Optional = isOptional;
    }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new WebConfigConfigurationProvider(this);
    }
}
