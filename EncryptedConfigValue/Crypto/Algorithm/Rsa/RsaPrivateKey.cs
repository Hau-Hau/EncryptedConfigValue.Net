using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using PrivateKey = Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters;

namespace EncryptedConfigValue.Crypto.Algorithm.Rsa
{
    public sealed class RsaPrivateKey : IKey
    {
        private readonly PrivateKey privateKey;

        public RsaPrivateKey(PrivateKey privateKey)
        {
            this.privateKey = privateKey;
        }

        public PrivateKey PrivateKey => privateKey;

        public byte[] Bytes => PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey).ToAsn1Object().GetDerEncoded();

        public class RsaPrivateKeyGenerator : IKeyGenerator
        {
            public static RsaPrivateKeyGenerator Instance { get; } = new RsaPrivateKeyGenerator();

            private RsaPrivateKeyGenerator()
            {
            }

            public KeyWithType KeyFromBytes(byte[] key)
            {
                AsymmetricKeyParameter privateKey = PrivateKeyFactory.CreateKey(key);
                var localPrivateKey = (RsaPrivateCrtKeyParameters)privateKey;
                return new KeyWithType(KeyType.RsaPrivate, new RsaPrivateKey(localPrivateKey));
            }
        }
    }
}