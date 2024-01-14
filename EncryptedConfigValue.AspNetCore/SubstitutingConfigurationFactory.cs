using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Util;
using Microsoft.Extensions.Configuration;

namespace EncryptedConfigValue.AspNetCore
{
    internal sealed class SubstitutingConfigurationFactory : IConfigurationSource
    {
        public IConfigurationRoot? Configuration { get; set; }
        public ISubstitutor? Substitutor { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (Configuration is null) throw new ArgumentNullException(nameof(Configuration));
            if (Substitutor is null) throw new ArgumentNullException(nameof(Substitutor));

            return new SubstitutingConfiguration(Configuration, Substitutor);
        }

        public class SubstitutingConfiguration : ConfigurationProvider
        {
            private readonly IConfigurationRoot configuration;
            private readonly ISubstitutor substitutor;

            public SubstitutingConfiguration(IConfigurationRoot configuration, ISubstitutor substitutor)
            {
                this.configuration = configuration;
                this.substitutor = substitutor;
            }
            public override void Load()
            {
                Data = new Dictionary<string, string?>();
                foreach (var provider in configuration.Providers)
                {
                    try
                    {
                        Data = Data.Concat(TraverseConfiguration(provider))
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                    catch (StringSubstitutionException e)
                    {
                        var pathToProviderSource = (provider as FileConfigurationProvider)?.Source.Path
                            ?? $"<{provider.GetType().Name}>";
                        throw new ConfigurationDecryptionException(pathToProviderSource, e);
                    }
                }
            }

            private Dictionary<string, string?> TraverseConfiguration(
                IConfigurationProvider provider,
                string? parentPath = null
            )
            {
                parentPath = string.IsNullOrEmpty(parentPath) ? null : parentPath;
                var result = new Dictionary<string, string?>();
                var childs = provider.GetChildKeys(Enumerable.Empty<string>(), parentPath);
                foreach (var key in childs.Distinct())
                {
                    var path = parentPath == null ? key : $"{parentPath}:{key}";
                    if (provider.TryGet(path, out var currentValue))
                    {
                        try
                        {
                            if (substitutor.TryReplace(currentValue, out var output))
                            {
                                result[path] = output;
                            }
                        }
                        catch (StringSubstitutionException e)
                        {
                            throw e.Extend(path);
                        }
                    }
                    else if (childs.Count(x => x == key) > 1)
                    {
                        result = result
                            .Concat(TraverseConfiguration(provider, path))
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                }
                return result;
            }
        }
    }
}
