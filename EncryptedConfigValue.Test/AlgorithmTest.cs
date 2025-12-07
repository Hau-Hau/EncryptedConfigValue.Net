using EncryptedConfigValue.Crypto.Algorithm;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace EncryptedConfigValueTest
{
    public class AlgorithmTest
    {
        private const string plaintext = "Some top secret plaintext for testing things";

        [Theory]
        [MemberData(nameof(Data))]
        internal void WeGenerateRandomKeys(Algorithm algorithm)
        {
            var keyPair1 = algorithm.NewKeyPair();
            var keyPair2 = algorithm.NewKeyPair();
            keyPair1.ShouldNotBe(keyPair2);
            keyPair1.ShouldNotBeSameAs(keyPair2);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void WeCanEncryptAndDecrypt(Algorithm algorithm)
        {
            var keyPair = algorithm.NewKeyPair();
            var encryptedValue = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);
            var decryptionKey = keyPair.DecryptionKey;
            var decrypted = encryptedValue.Decrypt(decryptionKey);
            decrypted.ShouldBe(plaintext);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void TheSameStringEncryptsToDifferentCiphertexts(Algorithm algorithm)
        {
            var keyPair = algorithm.NewKeyPair();
            var encrypted1 = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);
            var encrypted2 = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);

            // we don't want to leak that certain values are the same
            encrypted1.ShouldNotBe(encrypted2);
            encrypted1.ShouldNotBeSameAs(encrypted2);
            // paranoia, let's say the equals method is badly behaved
            encrypted1.GetHashCode().ShouldNotBe(encrypted2.GetHashCode());

            // we should naturally decrypt back to the same thing - the plaintext
            var decryptionKey = keyPair.DecryptionKey;
            var decryptedString1 = encrypted1.Decrypt(decryptionKey);
            var decryptedString2 = encrypted2.Decrypt(decryptionKey);

            decryptedString1.ShouldBe(plaintext);
            decryptedString2.ShouldBe(plaintext);
        }

        public static IEnumerable<object[]> Data() => new[]
        {
            new object[] { Algorithm.AES },
            new object[] { Algorithm.RSA },
        };
    }
}