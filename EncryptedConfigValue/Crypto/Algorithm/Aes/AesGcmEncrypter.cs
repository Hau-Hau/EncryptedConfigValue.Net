using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Text;

namespace EncryptedConfigValue.Crypto.Algorithm.Aes
{
    public sealed class AesGcmEncrypter : IEncrypter
    {
        public static AesGcmEncrypter Instance { get; } = new AesGcmEncrypter();

        private const int IvSizeBits = 96;
        private const int TagSizeBits = 128;

        public EncryptedValue Encrypt(KeyWithType kwt, string plaintext)
        {
            KeyType.Aes.CheckKeyArgument(kwt, typeof(AesKey));
            var secretKeySpec = ((AesKey)kwt.Key).Bytes;

            var ivBytes = new byte[IvSizeBits / 8];
            var secureRandom = new SecureRandom();
            secureRandom.NextBytes(ivBytes, 0, ivBytes.Length);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(secretKeySpec), TagSizeBits, ivBytes);
            cipher.Init(true, parameters);

            var encrypted = new byte[cipher.GetOutputSize(plaintext.Length)];
            var len = cipher.ProcessBytes(Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length, encrypted, 0);
            cipher.DoFinal(encrypted, len);

            // Tag is appended to ciphertext, so split apart manually
            byte[] ciphertext = new byte[encrypted.Length - (TagSizeBits / 8)];
            Array.Copy(encrypted, 0, ciphertext, 0, ciphertext.Length);

            byte[] tag = new byte[TagSizeBits / 8];
            Array.Copy(encrypted, encrypted.Length - (TagSizeBits / 8), tag, 0, tag.Length);

            return new AesEncryptedValue(ivBytes, ciphertext, tag);
        }
    }
}
