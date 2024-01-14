using EncryptedConfigValue.Converters;
using EncryptedConfigValue.Extensions;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using System;
using System.Text;

namespace EncryptedConfigValue.Crypto.Algorithm.Rsa
{
    /// <summary>
    /// Encrypts values using RSA-OAEP-MDF1. Uses SHA-256 as the hash function for both OAEP and MDF1.
    /// </summary>
    public sealed class RsaOaepEncrypter : IEncrypter
    {
        public static RsaOaepEncrypter Instance { get; } = new RsaOaepEncrypter();
        private static readonly HashAlgorithm OaepHashAlg = HashAlgorithm.SHA256;
        private static readonly HashAlgorithm Mdf1HashAlg = HashAlgorithm.SHA256;

        [JsonConverter(typeof(ToStringFromStringConverter<HashAlgorithm>))]
        public sealed class HashAlgorithm
        {
            public static readonly HashAlgorithm SHA1 = new HashAlgorithm("SHA-1");
            public static readonly HashAlgorithm SHA256 = new HashAlgorithm("SHA-256");

            public string Name { get; }

            private HashAlgorithm(string name)
            {
                Name = name;
            }

            public static HashAlgorithm FromString(string value)
            {
                switch (value)
                {
                    case "SHA-1": { return SHA1; }
                    case "SHA-256": { return SHA256; }
                    default: throw new NotSupportedException();
                }
            }

            public override string ToString() => Name;
        }

        public EncryptedValue Encrypt(KeyWithType kwt, string plaintext)
        {
            KeyType.RsaPublic.CheckKeyArgument(kwt, typeof(RsaPublicKey));
            var publicKey = ((RsaPublicKey)kwt.Key).PublicKey;

            var cipher = new OaepEncoding(new RsaEngine(), DigestUtilities.GetDigest(DigestUtilities.GetObjectIdentifier(OaepHashAlg.Name)), DigestUtilities.GetDigest(DigestUtilities.GetObjectIdentifier(Mdf1HashAlg.Name)), null);
            cipher.Init(true, publicKey);
            var encrypted = cipher.ApplyCipher(Encoding.UTF8.GetBytes(plaintext));
            return new RsaEncryptedValue(encrypted, OaepHashAlg, Mdf1HashAlg);
        }
    }
}