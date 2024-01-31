// MIT License.

using Xunit;

namespace Compiler.Dynamic.Tests;

/// <summary>
/// Used to disable parallelization on tests that are hosted since the hosting environment is stored as a static property.
/// </summary>
[CollectionDefinition(nameof(SelfHostedTests), DisableParallelization = true)]
public class SelfHostedTests
{
}
