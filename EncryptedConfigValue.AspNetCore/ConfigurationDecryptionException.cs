namespace EncryptedConfigValue.AspNetCore
{
    public sealed class ConfigurationDecryptionException : AggregateException
    {
        public ConfigurationDecryptionException(string path, params Exception[] innerExceptions)
            : base($"Configuration decryption error at {path}", innerExceptions) { }
    }
}
