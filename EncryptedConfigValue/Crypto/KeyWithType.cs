using EncryptedConfigValue.Converters;
using EncryptedConfigValue.Crypto.Algorithm;
using Newtonsoft.Json;
using System;

namespace EncryptedConfigValue.Crypto
{
    /// <summary>
    /// Stores a key with its type. Supports serializing to and from JSON as a string. The serialized representation is of
    /// the form "type:base64-encoded-key".
    /// </summary>
    [JsonConverter(typeof(ToStringFromStringConverter<KeyWithType>))]
    public sealed class KeyWithType
    {
        public KeyWithType(KeyType type, IKey key)
        {
            Type = type;
            Key = key;
        }

        public KeyType Type { get; }

        public IKey Key { get; }

        public override string ToString() => $"{Type}:{Convert.ToBase64String(Key.Bytes)}";

        public static KeyWithType FromString(string keyWithType)
        {
            if (!keyWithType.Contains(":"))
            {
                throw new Exception($"Key must be in the format <type>:<key in base64>");
            }

            var tokens = new Span<string>(keyWithType.Split(':'), 0, 2);
            byte[] decodedKey = Convert.FromBase64String(tokens[1]);

            // legacy RSA key format
            if (tokens[0].Equals("RSA"))
            {
                // try parsing as private key
                Exception privateKeyException;
                try
                {
                    return KeyType.RsaPrivate.KeyFromBytes(decodedKey);
                }
                catch (Exception e)
                {
                    // ignore; try parsing as public key
                    privateKeyException = e;
                }

                // try parsing as public key
                Exception publicKeyException;
                try
                {
                    return KeyType.RsaPublic.KeyFromBytes(decodedKey);
                }
                catch (Exception e)
                {
                    // ignore; try parsing as public key
                    publicKeyException = e;
                }

                throw new AggregateException("unable to parse legacy RSA key.", privateKeyException, publicKeyException);
            }

            KeyType keyAlg = KeyType.From(tokens[0]);
            return keyAlg.KeyFromBytes(decodedKey);
        }
    }
}