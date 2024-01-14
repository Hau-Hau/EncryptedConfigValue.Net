namespace EncryptedConfigValue.Crypto
{
    public interface ISubstitutor
    {
        bool TryReplace(string source, out string output);
    }
}
