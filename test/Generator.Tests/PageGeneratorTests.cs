using Xunit;

using VerifyCS = Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.Tests.GeneratorVerifier<
    Microsoft.AspNetCore.SystemWebAdapters.UI.Generator.PageGenerator>;

namespace SystemWebAdapters.UI.Generator.Tests;

public class PageGeneratorTests
{
    [Fact]
    public async Task Empty()
    {
        await new VerifyCS.Test().RunAsync();
    }
}