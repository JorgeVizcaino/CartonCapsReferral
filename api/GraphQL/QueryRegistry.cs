using HotChocolate.Execution.Configuration;

namespace api.GraphQL
{
    public static class QueryRegistry
    {
        public static IRequestExecutorBuilder RegisterQueries(this IRequestExecutorBuilder builder)
        {
            return builder
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<ReferralsQueries>();
        }
    }
}
