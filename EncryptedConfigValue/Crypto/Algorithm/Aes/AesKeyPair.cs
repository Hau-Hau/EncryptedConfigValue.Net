using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace EncryptedConfigValue.Crypto.Algorithm.Aes
{
    public static class AesKeyPair
    {
        private const int KeySizeBits = 256;

        public static KeyPair NewKeyPair()
        {
            var keyGen = new CipherKeyGenerator();
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), KeySizeBits));
            var secretKey = keyGen.GenerateKey();

            var kwa = new KeyWithType(KeyType.Aes, new AesKey(secretKey));
            return KeyPair.Symmetric(kwa);
        }
    }
}
