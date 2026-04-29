using Microsoft.Extensions.DependencyInjection;
using HiddenGemShared.Interfaces;
using HiddenGemDAL.Repositories;

namespace HiddenGemDAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHiddenGemData(this IServiceCollection services, string keywordsPath)
        {
            services.AddScoped<ICardRepository, SurrealCardRepository>();
            return services;
        }
    }
}