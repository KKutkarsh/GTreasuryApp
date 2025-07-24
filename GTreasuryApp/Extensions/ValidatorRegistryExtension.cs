using GTreasury.Api.Functions.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GTreasury.Api.Functions.Extensions
{
    internal static  class ValidatorRegistryExtension
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddTransient<CashFlowValidator>();
            services.AddTransient<NpvInputValidator>();

            return services;
        }
    }
}
