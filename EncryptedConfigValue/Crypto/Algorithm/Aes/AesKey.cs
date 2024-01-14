namespace EncryptedConfigValue.Crypto.Algorithm.Aes
{
    public sealed class AesKey : IKey
    {
        public AesKey(byte[] secretKey)
        {
            this.Bytes = secretKey;
        }

        public byte[] Bytes { get; }

        public class AesKeyGenerator : IKeyGenerator
        {
            public static AesKeyGenerator Instance { get; } = new AesKeyGenerator();

            private AesKeyGenerator() { }

            public KeyWithType KeyFromBytes(byte[] key)
            {
                return new KeyWithType(KeyType.Aes, new AesKey(key));
            }
        }
    }
}
