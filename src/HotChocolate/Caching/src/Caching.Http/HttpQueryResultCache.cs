using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;

namespace HotChocolate.Caching.Http;

public class HttpQueryResultCache : DefaultQueryResultCache
{
    private static readonly string _httpContextKey = nameof(HttpContext);
    private static readonly string _cacheControlValueTemplate = "{0}, max-age={1}";

    public override Task<IQueryResult?> TryReadCachedQueryResultAsync(IRequestContext context)
    {
        // the cache request is supposed to be handled by a CDN or another inbetween HTTP caching layer
        // so we should never actually get here - and if we do, we do nothing.

        return Task.FromResult<IQueryResult?>(null);
    }

    public override Task CacheQueryResultAsync(IRequestContext context, QueryResultCacheSettings settings)
    {
        if (!settings.CanBeCached)
        {
            return Task.CompletedTask;
        }

        if (!context.ContextData.TryGetValue(_httpContextKey, out var httpContextValue)
            || httpContextValue is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        var cacheType = settings.IsPrivate ? "private" : "public";

        var headerValue = string.Format(_cacheControlValueTemplate, cacheType, settings.MaxAge.TotalSeconds);

        // todo: the new header setters exist, but are not usable?
        httpContext.Response.Headers.Add("Cache-Control", headerValue);

        return Task.CompletedTask;
    }
}