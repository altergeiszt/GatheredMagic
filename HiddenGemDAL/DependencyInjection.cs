using Microsoft.Extensions.DependencyInjection;
using SurrealDb.Net;
using HiddenGemShared.Interfaces;
using HiddenGemDAL.Repositories;

namespace HiddenGemDAL;

public static class DependencyInjection
{
    public static IServiceCollection AddHiddenGemData(this IServiceCollection services, string endpoint, string user, string pass)
    {
        // Register SurrealDB Client
        var options = SurrealDbOptions.Create().WithEndpoint(endpoint).WithNamespace("hidden_gem_ns").WithDatabase("main_db").WithUsername(user).WithPassword(pass).Build();

        services.AddSurreal(options, ServiceLifetime.Scoped);
        return services;
    }
}
