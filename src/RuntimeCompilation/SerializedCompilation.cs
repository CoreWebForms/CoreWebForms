// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters.UI.RuntimeCompilation;

internal sealed class SerializedCompilation : BackgroundService
{
    private readonly IQueue _queue;
    private readonly ILogger<SerializedCompilation> _logger;

    public SerializedCompilation(IQueue queue, ILogger<SerializedCompilation> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _queue.GetItemsAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Running compilation task");
                await item(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Finished compilation task");
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogError(e, "Unexpected error compiling");
            }
        }
    }
}
