using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Xunit;
using FluentAssertions;
using EncryptedConfigValue.Crypto.Algorithm;

namespace EncryptedConfigValue.Test.Rsa
{
    public class RsaPublicKeyTest
    {
        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator(Algorithm.RSA.ToString());
            keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            return keyPairGenerator.GenerateKeyPair();
        }

        [Fact]
        public void TestEqualityFromSamePublicKey()
        {
            var privateKey = (RsaKeyParameters)GenerateKeyPair().Public;
            new RsaPublicKey(privateKey).Should().BeEquivalentTo(new RsaPublicKey(privateKey));
        }
    }
}
