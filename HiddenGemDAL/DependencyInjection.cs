using Microsoft.Extensions.DependencyInjection;
using SurrealDb.Net;
using HiddenGemShared.Interfaces;
using HidddenGemDAL.Repositories;

namespace HidddenGemDAL;

public class DependencyInjection
{
    public static IServiceCollection AddHiddenGemData(this IServiceCollection services, string endpoint, string user, string pass)
    {
        // Register SurrealDB Client
        services.AddSurrealDb(endpoint, "hidden_gem_ns", "main_db",(builder) =>
        {
            builder.WithUsername(user).WithPassword(pass);
        });

        // Register the Repositories so the BLL can talk to it via Interface
        services.AddScoped<ICardRepository, SurrealCardRepository>();

        return services;
    }
}
