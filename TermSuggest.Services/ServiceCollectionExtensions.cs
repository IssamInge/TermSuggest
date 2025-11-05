using Microsoft.Extensions.DependencyInjection;
using TermSuggest.Core;

namespace TermSuggest.Services
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extensions pour configurer les services dans un conteneur DI
        /// </summary>
        public static IServiceCollection AddTermSuggestServices(this IServiceCollection services)
        {
            services.AddScoped<ITermMatcher, TermMatcherService>();
            return services;
        }
    }
}
