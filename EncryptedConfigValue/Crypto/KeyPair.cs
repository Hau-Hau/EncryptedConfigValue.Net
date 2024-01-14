namespace EncryptedConfigValue.Crypto
{
    /// <summary>
    /// An encryption and decryption key. For symmetric keys, both keys will be the same.
    /// </summary>
    public sealed class KeyPair
    {
        private KeyPair(KeyWithType encryptionKey, KeyWithType decryptionKey)
        {
            EncryptionKey = encryptionKey;
            DecryptionKey = decryptionKey;
        }

        public KeyWithType EncryptionKey { get; }

        public KeyWithType DecryptionKey { get; }

        public static KeyPair Of(KeyWithType encryptionKey, KeyWithType decryptionKey)
        {
            return new KeyPair(encryptionKey, decryptionKey);
        }

        public static KeyPair Symmetric(KeyWithType key)
        {
            return new KeyPair(key, key);
        }
    }
}