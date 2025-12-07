using EncryptedConfigValue.Crypto.Algorithm.Aes;
using Shouldly;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Xunit;

namespace EncryptedConfigValue.Test.Aes
{
    public class AesKeyTest
    {
        public static byte[] NewSecretKey()
        {
            var keyGen = new CipherKeyGenerator();
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), 128));
            return keyGen.GenerateKey();
        }

        [Fact]
        public void TestEqualityFromSameSecretKey()
        {
            var secretKey = NewSecretKey();
            new AesKey(secretKey).ShouldBeEquivalentTo(new AesKey(secretKey));
        }
    }
}
