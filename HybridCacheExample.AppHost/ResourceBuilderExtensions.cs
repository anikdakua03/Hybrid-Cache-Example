using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace HybridCacheExample.AppHost;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithSwaggerUI<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints
    {
        return builder.WithOpenAPIDocs("swagger-ui-docs", "Swagger API Documentation", "swagger");
    }

    public static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints
    {
        return builder.WithOpenAPIDocs("scalar-docs", "Scalar API Documentation", "scalar/v1");
    }

    private static IResourceBuilder<T> WithOpenAPIDocs<T>(this IResourceBuilder<T> builder, string name, string displayName, string openApiUIPath) where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
            name,
            displayName,
            executeCommand: async _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");

                    var url = $"{endpoint.Url}/{openApiUIPath}";

                    Process.Start(new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    });

                    return new ExecuteCommandResult() { Success = true };
                }
                catch (Exception ex)
                {
                    return new ExecuteCommandResult()
                    {
                        Success = false,
                        ErrorMessage = ex.ToString()
                    };
                }
            },
            updateState: context =>
            {
                return context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ? ResourceCommandState.Enabled : ResourceCommandState.Disabled;
            },
            iconName: "Document",
            iconVariant: IconVariant.Filled);
    }
}
