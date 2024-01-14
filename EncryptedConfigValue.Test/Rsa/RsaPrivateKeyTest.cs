using EncryptedConfigValue.Crypto.Algorithm;
using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using FluentAssertions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Xunit;

namespace EncryptedConfigValue.Test.Rsa
{
    public class RsaPrivateKeyTest
    {
        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator(Algorithm.RSA.ToString());
            keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            return keyPairGenerator.GenerateKeyPair();
        }

        [Fact]
        public void TestEqualityFromSamePrivateKey()
        {
            var privateKey = (RsaKeyParameters)GenerateKeyPair().Private;
            new RsaPrivateKey(privateKey).Should().BeEquivalentTo(new RsaPrivateKey(privateKey));
        }
    }
}
