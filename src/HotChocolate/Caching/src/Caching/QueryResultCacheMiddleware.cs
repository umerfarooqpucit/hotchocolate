using System;
using System.Threading.Tasks;
using HotChocolate.Execution;

namespace HotChocolate.Caching;

public sealed class QueryResultCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IQueryResultCache _cache;

    public QueryResultCacheMiddleware(RequestDelegate next, IQueryResultCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async ValueTask InvokeAsync(IRequestContext context)
    {
        if (_cache.ShouldReadResultFromCache(context))
        {
            // todo: where to get cancellationtoken from
            IQueryResult? cachedResult = await _cache.TryReadCachedQueryResultAsync(context);

            if (cachedResult is not null)
            {
                // todo: return result served from cache
            }
        }

        await _next(context).ConfigureAwait(false);

        if (_cache.ShouldWriteResultToCache(context))
        {
            QueryResultCacheSettings settings = GetSettingsFromContext(context);

            await _cache.CacheQueryResultAsync(context, settings);
        }
    }

    private static QueryResultCacheSettings GetSettingsFromContext(IRequestContext context)
    {
        return new QueryResultCacheSettings
        {
            MaxAge = TimeSpan.FromSeconds(2000)
        };
    }
}