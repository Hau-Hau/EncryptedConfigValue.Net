using EncryptedConfigValue.Converters;
using EncryptedConfigValue.Crypto.Algorithm.Aes;
using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EncryptedConfigValue.Crypto.Algorithm
{
    /// <summary>
    /// Defines the known encryption algorithms. Algorithms can generate a new <see cref="KeyPair"/> that contains encryption and
    /// decryption keys for the algorithm and can return an <see cref="IEncrypter"/>
    /// that can be used to encrypt values using this
    /// algorithm with a supported key.
    /// </summary>
    [JsonConverter(typeof(ToStringFromStringConverter<Algorithm>))]
    public sealed class Algorithm
    {
        public static readonly Algorithm AES = new Algorithm("AES", AesGcmEncrypter.Instance, () => AesKeyPair.NewKeyPair());
        public static readonly Algorithm RSA = new Algorithm("RSA", RsaOaepEncrypter.Instance, () => RsaKeyPair.NewKeyPair());
        private static readonly IReadOnlyDictionary<string, Algorithm> Values = new Dictionary<string, Algorithm>
        {
            ["AES"] = AES,
            ["RSA"] = RSA
        };

        private readonly Func<KeyPair> getKeyPairFunc;
        private readonly string name;
        private readonly IEncrypter encrypter;

        private Algorithm(string name, IEncrypter encrypter, Func<KeyPair> getKeyPairFunc)
        {
            this.name = name;
            this.encrypter = encrypter;
            this.getKeyPairFunc = getKeyPairFunc;
        }

        public static Algorithm FromString(string name)
        {
            if (!Values.TryGetValue(name, out var output))
            {
                throw new ArgumentException($"Algorithm {name} doesn't exists");
            }
            return output;
        }

        public KeyPair NewKeyPair() => getKeyPairFunc.Invoke();

        public IEncrypter NewEncrypter() => encrypter;

        public override string ToString() => name;
    }
}