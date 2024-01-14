using EncryptedConfigValue.Crypto.Algorithm.Aes;
using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using System;
using static EncryptedConfigValue.Crypto.Algorithm.Rsa.RsaOaepEncrypter;

namespace EncryptedConfigValue.Crypto
{
    public sealed class LegacyEncryptedValue : EncryptedValue
    {
        public LegacyEncryptedValue(byte[] cipherText)
        {
            CipherText = cipherText;
        }

        public byte[] CipherText { get; }

        public override string Decrypt(KeyWithType kwa)
        {
            EncryptedValue translatedValue;
            if (kwa.Key is AesKey)
            {
                // if AES key is provided, interpret value as legacy AES value
                translatedValue = AesValueFromLegacy(this);
            }
            else if (kwa.Key is RsaPrivateKey)
            {
                // if AES key is provided, interpret value as legacy AES value
                translatedValue = RsaValueFromLegacy(this);
            }
            else
            {
                throw new ArgumentException(
                        $"decrypting legacy values not supported for key type {kwa.Type}");
            }

            return translatedValue.Decrypt(kwa);
        }

        private static readonly int LegacyIvSize = 256 / 8;
        private static readonly int LegacyTagSize = 128 / 8;

        private static AesEncryptedValue AesValueFromLegacy(LegacyEncryptedValue value)
        {
            var buf = value.CipherText;
            byte[] iv = new byte[LegacyIvSize];
            Array.Copy(buf, 0, iv, 0, LegacyIvSize);
            byte[] ct = new byte[buf.Length - LegacyIvSize - LegacyTagSize];
            Array.Copy(buf, LegacyIvSize, ct, 0, ct.Length);
            byte[] tag = new byte[LegacyTagSize];
            Array.Copy(buf, buf.Length - LegacyTagSize, tag, 0, LegacyTagSize);
            return new AesEncryptedValue(iv, ct, tag);
        }

        private static RsaEncryptedValue RsaValueFromLegacy(LegacyEncryptedValue value)
        {
            return new RsaEncryptedValue(
                value.CipherText,
                HashAlgorithm.SHA256,
                HashAlgorithm.SHA1);
        }

        public override T Accept<T>(EncryptedValueVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}