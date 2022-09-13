// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal interface IQueue
{
    void Add(Func<CancellationToken, Task> func);

    IAsyncEnumerable<Func<CancellationToken, Task>> GetItemsAsync(CancellationToken token);
}
