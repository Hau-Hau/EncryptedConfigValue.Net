using EncryptedConfigValue.Module;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace EncryptedConfigValue.AspNetCore
{
    public static class EncryptedConfigValue
    {
        public static IConfigurationBuilder AddEncryptedConfigValueProvider(
            this IConfigurationBuilder builder
        )
        {
            var configurationSnapshot = builder.Build();
            return builder.Add(
                new SubstitutingConfigurationFactory
                {
                    Configuration = configurationSnapshot,
                    Substitutor = new DecryptingVariableSubstitutor()
                });
        }

        public static WebApplicationBuilder AddEncryptedConfigValueProvider(
            this WebApplicationBuilder builder
        )
        {
            builder.Configuration.Add<SubstitutingConfigurationFactory>(
                (x) =>
                {
                    x.Configuration = builder.Configuration;
                    x.Substitutor = new DecryptingVariableSubstitutor();
                });
            return builder;
        }
    }
}
