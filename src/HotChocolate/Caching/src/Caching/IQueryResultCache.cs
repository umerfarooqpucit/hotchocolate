using HotChocolate.Execution;
using System.Threading.Tasks;

namespace HotChocolate.Caching;

public interface IQueryResultCache
{
    bool ShouldReadResultFromCache(IRequestContext context);

    bool ShouldWriteResultToCache(IRequestContext context);

    Task<IQueryResult?> TryReadCachedQueryResultAsync(IRequestContext context);

    Task CacheQueryResultAsync(IRequestContext context, QueryResultCacheSettings settings);
}