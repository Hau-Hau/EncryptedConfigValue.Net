using EncryptedConfigValue.Extensions;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using System.Text;
using System.Text.Json.Serialization;
using static EncryptedConfigValue.Crypto.Algorithm.Rsa.RsaOaepEncrypter;

namespace EncryptedConfigValue.Crypto.Algorithm.Rsa
{
    public sealed class RsaEncryptedValue : EncryptedValue
    {
        public enum Mode
        {
            OAEP,
        }

        [JsonConstructor]
        public RsaEncryptedValue(byte[] cipherText, HashAlgorithm oaepHashAlg, HashAlgorithm mdf1HashAlg)
        {
            CipherText = cipherText;
            OaepHashAlg = oaepHashAlg;
            Mdf1HashAlg = mdf1HashAlg;
        }

        //[JsonPropertyOrder(int.MinValue)]
        public Algorithm Type => Algorithm.RSA;

        // Returns the encryption mode used by this encrypted value.
        public Mode EncryptionMode => Mode.OAEP;

        public byte[] CipherText { get; }

        [JsonPropertyName("oaep-alg")]
        public HashAlgorithm OaepHashAlg { get; }

        [JsonPropertyName("mdf1-alg")]
        public HashAlgorithm Mdf1HashAlg { get; }

        public override string Decrypt(KeyWithType kwt)
        {
            KeyType.RsaPrivate.CheckKeyArgument(kwt, typeof(RsaPrivateKey));
            var privateKey = ((RsaPrivateKey)kwt.Key).PrivateKey;

            var cipher = new OaepEncoding(new RsaEngine(), DigestUtilities.GetDigest(DigestUtilities.GetObjectIdentifier(OaepHashAlg.Name)), DigestUtilities.GetDigest(DigestUtilities.GetObjectIdentifier(Mdf1HashAlg.Name)), null);
            cipher.Init(false, privateKey);
            var blockSize = privateKey.Modulus.BitLength / 8;
            var decryptedData = cipher.ApplyCipher(CipherText, blockSize);
            return Encoding.UTF8.GetString(decryptedData);
        }

        public override T Accept<T>(EncryptedValueVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}