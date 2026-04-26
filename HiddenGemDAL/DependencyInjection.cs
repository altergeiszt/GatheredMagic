using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SurrealDb.Net;
using HiddenGemShared.Interfaces;
using HiddenGemDAL.Repositories;
using HiddenGemBLL.Services;

namespace HiddenGemDAL;

public static class DependencyInjection
{
    // Added 'keywordsPath' as a parameter
    public static IServiceCollection AddHiddenGemData(this IServiceCollection services, string endpoint, string user, string pass, string keywordsPath)
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

        // Register the Normalizer using the path provided by the Host
        services.AddSingleton<ICardNormalizerService>(provider => 
            new CardNormalizerService(keywordsPath));
            
        return services;
    }
}