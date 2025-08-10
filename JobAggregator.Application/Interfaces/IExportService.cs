using System;
using System.Collections.Generic;
using System.Threading;

namespace JobAggregator.Application.Interfaces
{
    public interface IExportService
    {
        IAsyncEnumerable<string> StreamAppliedJobsCsvAsync(Guid userId, CancellationToken cancellationToken);
    }
}
