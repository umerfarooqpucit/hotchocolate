using System;

namespace HotChocolate.Caching;

public class QueryResultCacheSettings
{
    public TimeSpan MaxAge { get; set; }

    public bool IsPrivate { get; set; }

    public bool CanBeCached => MaxAge.TotalSeconds > 0;
}