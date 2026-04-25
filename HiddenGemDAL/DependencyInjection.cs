using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SurrealDb.Net;
using HiddenGemShared.Interfaces;
using HiddenGemDAL.Repositories;
using HiddenGemBLL.Services;

namespace HiddenGemDAL;

public static class DependencyInjection
{
    public static IServiceCollection AddHiddenGemData(this IServiceCollection services, string endpoint, string user, string pass)
    {
        // Register SurrealDB Client
        var options = SurrealDbOptions.Create()
        .WithEndpoint(endpoint)
        .WithNamespace("hidden_gem_ns")
        .WithDatabase("main_db")
        .WithUsername(user)
        .WithPassword(pass)
        .Build();

        services.AddSurreal(options, ServiceLifetime.Scoped);

        string keywordsPath = Path.Combine(AppContext.BaseDirectorym "HiddenGemResources","Keywords.json");

        services.AddSingleton<ICardNormalizerService>(provider => 
        new CardNormalizerService(keywordsPath));
        return services;
    }
}
