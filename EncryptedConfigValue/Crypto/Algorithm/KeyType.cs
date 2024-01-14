using System;
using static EncryptedConfigValue.Crypto.Algorithm.Aes.AesKey;
using static EncryptedConfigValue.Crypto.Algorithm.Rsa.RsaPrivateKey;
using static EncryptedConfigValue.Crypto.Algorithm.Rsa.RsaPublicKey;

namespace EncryptedConfigValue.Crypto.Algorithm
{
    /// <summary>
    /// KeyType defines the universe of available key types. Each key type has a unique name and supports creating a new
    /// <see cref="KeyWithType"/> based on key bytes.
    /// </summary>
    public sealed class KeyType
    {
        public static readonly KeyType Aes = new KeyType("AES", AesKeyGenerator.Instance, Algorithm.AES);
        public static readonly KeyType RsaPublic = new KeyType("RSA-PUB", RsaPublicKeyGenerator.Instance, Algorithm.RSA);
        public static readonly KeyType RsaPrivate = new KeyType("RSA-PRIV", RsaPrivateKeyGenerator.Instance, Algorithm.RSA);
        private static readonly KeyType[] values = new KeyType[] { Aes, RsaPublic, RsaPrivate };

        public static KeyType From(string name)
        {
            foreach (var alg in values)
            {
                if (alg.name == name)
                {
                    return alg;
                }
            }
            throw new ArgumentException($"unrecognized key algorithm: {name}");
        }

        private readonly string name;
        private readonly IKeyGenerator generator;

        private KeyType(string name, IKeyGenerator generator, Algorithm algorithm)
        {
            this.name = name;
            this.generator = generator;
            Algorithm = algorithm;
        }

        public override string ToString() => name;

        public KeyWithType KeyFromBytes(byte[] keyBytes) => generator.KeyFromBytes(keyBytes);

        public Algorithm Algorithm { get; }

        public void CheckKeyArgument(KeyWithType kwt, Type keyType)
        {
            if (kwt.Type != this)
            {
                throw new ArgumentException($"key type did not match expected type for algorithm\nalgorith: {name}\ntype: {kwt.Type.name}");
            }
            if (!keyType.IsAssignableFrom(kwt.Key.GetType()))
            {
                throw new ArgumentException($"key type did not match expected type\nexpected: {keyType.Name}\n actual: {kwt.Key.GetType().Name}");
            }
        }
    }
}