using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Caching.Http.Tests;

public class HttpCachingTests : ServerTestBase
{
    public HttpCachingTests(TestServerFactory serverFactory)
            : base(serverFactory)
    {
    }

    [Fact]
    public async Task Test()
    {
        TestServer server = CreateServer(services =>
        {
            services.AddSingleton<IQueryResultCache, HttpQueryResultCache>();

            services.AddGraphQLServer()
                .AddQueryType<Query>()
                .UseRequest<QueryResultCacheMiddleware>()
                .UseDefaultPipeline();
        });

        HttpClient client = server.CreateClient();

        var result = await client.PostQueryAsync("{ field1 }");

        result.MatchSnapshot();
    }

    public class Query
    {
        public string Field1() => "Test";
    }
}

public class GraphQLResult
{
    public HttpResponseHeaders Headers { get; set; }

    public HttpContentHeaders ContentHeaders { get; set; }

    public string Body { get; set; }
}

internal static class TestServerExtensions
{
    public static async Task<GraphQLResult> PostQueryAsync(this HttpClient client, string query)
    {
        var payload = $"{{ \"query\": \"{query}\" }}";

        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/graphql", content);

        var result = new GraphQLResult
        {
            Headers = response.Headers,
            ContentHeaders = response.Content.Headers,
            Body = await response.Content.ReadAsStringAsync()
        };

        return result;
    }
}