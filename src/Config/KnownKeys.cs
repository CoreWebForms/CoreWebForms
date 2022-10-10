// MIT License.

using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration;

internal record KnownKeys(IEnumerable<string> AppSettings, IEnumerable<string> ConnectionStrings);
