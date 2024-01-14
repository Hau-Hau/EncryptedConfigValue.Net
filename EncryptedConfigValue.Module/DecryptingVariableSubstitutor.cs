using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Util;
using System;
using System.Text.RegularExpressions;

namespace EncryptedConfigValue.Module
{
    public sealed class DecryptingVariableSubstitutor : ISubstitutor
    {
        private static readonly Regex Pattern = new Regex("\\$\\{(enc:.*?)}");

        public bool TryReplace(string source, out string output)
        {
            output = source;
            if (source != null && source.Contains("${"))
            {
                output = Pattern.Replace(
                    source,
                    matchResult =>
                    {
                        string encryptedValue = matchResult.Groups[1].Value;
                        try
                        {
                            // No need for quoteReplacement as C# don't evaluate replacement string as a regex
                            return KeyFileUtils.DecryptUsingDefaultKeys(EncryptedValue.FromString(encryptedValue));
                        }
                        catch (Exception e)
                        {
                            throw new StringSubstitutionException(e, encryptedValue);
                        }
                    }
                );
                return output != null;
            }
            return false;
        }
    }
}
