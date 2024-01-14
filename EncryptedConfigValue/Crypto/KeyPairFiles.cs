namespace EncryptedConfigValue.Crypto
{
    public sealed class KeyPairFiles
    {
        public string EncryptionKeyFile { get; }

        public string DecryptionKeyFile { get; }

        public KeyPairFiles(string encryptionKeyFile, string decryptionKeyFile)
        {
            EncryptionKeyFile = encryptionKeyFile;
            DecryptionKeyFile = decryptionKeyFile;
        }

        public bool PathsEqual()
        {
            return EncryptionKeyFile == DecryptionKeyFile;
        }
    }
}
