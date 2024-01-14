using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using PublicKey = Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters;

namespace EncryptedConfigValue.Crypto.Algorithm.Rsa
{
    public sealed class RsaPublicKey : IKey
    {
        private readonly PublicKey publicKey;

        public RsaPublicKey(PublicKey publicKey)
        {
            this.publicKey = publicKey;
        }

        public PublicKey PublicKey => publicKey;

        public byte[] Bytes => SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey).GetEncoded();

        public class RsaPublicKeyGenerator : IKeyGenerator
        {
            public static RsaPublicKeyGenerator Instance { get; } = new RsaPublicKeyGenerator();

            private RsaPublicKeyGenerator()
            {
            }

            public KeyWithType KeyFromBytes(byte[] key)
            {
                PublicKey localPublicKey = null;
                try
                {
                    AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(key);
                    localPublicKey = (RsaKeyParameters)publicKey;
                }
                catch
                {
                }
                return new KeyWithType(KeyType.RsaPublic, new RsaPublicKey(localPublicKey));
            }
        }
    }
}