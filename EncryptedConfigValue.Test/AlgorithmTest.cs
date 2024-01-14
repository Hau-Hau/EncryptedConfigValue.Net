using EncryptedConfigValue.Crypto.Algorithm;
using FluentAssertions;
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
            keyPair1.Should().NotBe(keyPair2).And.NotBeSameAs(keyPair2);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void WeCanEncryptAndDecrypt(Algorithm algorithm)
        {
            var keyPair = algorithm.NewKeyPair();
            var encryptedValue = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);
            var decryptionKey = keyPair.DecryptionKey;
            var decrypted = encryptedValue.Decrypt(decryptionKey);
            decrypted.Should().Be(plaintext);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void TheSameStringEncryptsToDifferentCiphertexts(Algorithm algorithm)
        {
            var keyPair = algorithm.NewKeyPair();
            var encrypted1 = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);
            var encrypted2 = algorithm.NewEncrypter().Encrypt(keyPair.EncryptionKey, plaintext);

            // we don't want to leak that certain values are the same
            encrypted1.Should().NotBe(encrypted2).And.NotBeSameAs(encrypted2);
            // paranoia, let's say the equals method is badly behaved
            encrypted1.GetHashCode().Should().NotBe(encrypted2.GetHashCode());

            // we should naturally decrypt back to the same thing - the plaintext
            var decryptionKey = keyPair.DecryptionKey;
            var decryptedString1 = encrypted1.Decrypt(decryptionKey);
            var decryptedString2 = encrypted2.Decrypt(decryptionKey);

            decryptedString1.Should().Be(plaintext);
            decryptedString2.Should().Be(plaintext);
        }

        public static IEnumerable<object[]> Data() => new[]
        {
            new object[] { Algorithm.AES },
            new object[] { Algorithm.RSA },
        };
    }
}