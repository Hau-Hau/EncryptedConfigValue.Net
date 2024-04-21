using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace EncryptedConfigValue.Crypto.Algorithm.Rsa
{
    public static class RsaKeyPair
    {
        private const int KeySizeBits = 2048;

        public static KeyPair NewKeyPair()
        {
            var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator(Algorithm.RSA.ToString());
            keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KeySizeBits));
            var rsaKeyPair = keyPairGenerator.GenerateKeyPair();

            var pub = new KeyWithType(
                type: KeyType.RsaPublic,
                key: new RsaPublicKey((RsaKeyParameters)rsaKeyPair.Public)
            );
            var priv = new KeyWithType(
                type: KeyType.RsaPrivate,
                key: new RsaPrivateKey((RsaKeyParameters)rsaKeyPair.Private)
            );
            return KeyPair.Of(pub, priv);
        }
    }
}