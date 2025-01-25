using ErrorOr;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Mimo.Challenge.Domain.Abstractions;

namespace Mimo.Challenge.Features.ResetCache;

public record ResetCacheRequest : IRequest<ErrorOr<Success>>;


//memoryCache must be registered as singleton in DI
public class ResetCacheHandler(MemoryCache memoryCache) 
    : IRequestHandler<ResetCacheRequest, ErrorOr<Success>>
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<ErrorOr<Success>> Handle(ResetCacheRequest request, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            memoryCache.Clear();
            return Result.Success;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}