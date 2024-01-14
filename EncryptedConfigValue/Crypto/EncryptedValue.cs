using EncryptedConfigValue.Converters;
using EncryptedConfigValue.Crypto.Algorithm.Aes;
using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace EncryptedConfigValue.Crypto
{
    /// <summary>
    /// A value that has been encrypted using an algorithm with specific parameters. The value can be decrypted when provided
    /// with a key that has a type that is capable of performing decryption for the algorithm used to encrypt this value.
    /// The serializable String form is "enc:base64-encoded-value".
    ///
    /// An <see cref="EncryptedValue"/> has a legacy format and a current format.
    ///
    /// In the legacy format, the base64-encoded-value is the base64-encoded ciphertext bytes. The value does not contain any
    /// information about the algorithm or parameters used to encrypt it and blindly uses any key provided to attempt to
    /// decrypt the ciphertext.
    ///
    /// In the current format, the base64-encoded-value is the base64-encoded JSON representation of the concrete
    /// <see cref="EncryptedValue"/> subclass of the value. The subclass contains information about the algorithm used to encrypt
    /// the value, along with any relevant parameters for the algorithm.
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), nameof(Type))]
    [JsonSubtypes.KnownSubType(typeof(AesEncryptedValue), "AES")]
    [JsonSubtypes.KnownSubType(typeof(RsaEncryptedValue), "RSA")]
    public abstract class EncryptedValue
    {
        private const string PREFIX = "enc:";

        public abstract T Accept<T>(EncryptedValueVisitor<T> visitor);

        public abstract string Decrypt(KeyWithType kwt);

        public static bool IsEncryptedValue(string value)
        {
            return value.StartsWith(PREFIX);
        }

        public static EncryptedValue FromString(string value)
        {
            if (!IsEncryptedValue(value))
            {
                throw new ArgumentException($"Missing \"enc:\" prefix: {value}");
            }

            var suffix = value.Substring(PREFIX.Length);
            var bytes = Convert.FromBase64String(suffix);
            try
            {
                using (var reader = new MemoryStream(bytes))
                {
                    var instance = JsonConvert.DeserializeObject<EncryptedValue>(Encoding.UTF8.GetString(bytes));
                    return instance ?? throw new ArgumentException("Deserialized encrypted value object is null");
                }
            }
            catch
            {
                return new LegacyEncryptedValue(bytes);
            }
        }

        public override string ToString()
        {
            byte[] bytes = Accept(new EncryptedValueVisitor<byte[]>(
                (legacyEncryptedValue) => legacyEncryptedValue.CipherText,
            (aesEncryptedValue) => GetJsonBytes(aesEncryptedValue),
                (rsaEncryptedValue) => GetJsonBytes(rsaEncryptedValue))
            );
            return PREFIX + Convert.ToBase64String(bytes);
        }

        private static byte[] GetJsonBytes(object value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        }
    }
}
