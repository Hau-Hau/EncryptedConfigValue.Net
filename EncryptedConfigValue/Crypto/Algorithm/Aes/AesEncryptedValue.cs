using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace EncryptedConfigValue.Crypto.Algorithm.Aes
{
    public sealed class AesEncryptedValue : EncryptedValue
    {
        public enum Mode
        {
            GCM,
        }

        [JsonConstructor]
        public AesEncryptedValue(byte[] iv, byte[] cipherText, byte[] tag)
        {
            Iv = iv;
            CipherText = cipherText;
            Tag = tag;
        }

        public Algorithm Type => Algorithm.AES;

        // Returns the encryption mode used by this encrypted value.
        public Mode EncryptionMode => Mode.GCM;

        public byte[] Iv { get; }

        public byte[] CipherText { get; }

        public byte[] Tag { get; }

        public override string Decrypt(KeyWithType kwt)
        {
            using (var cipherStream = new MemoryStream(CipherText))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                KeyType.Aes.CheckKeyArgument(kwt, typeof(AesKey));
                var secretKeySpec = ((AesKey)kwt.Key).Bytes;

                byte[] ct = new byte[CipherText.Length + Tag.Length];
                Buffer.BlockCopy(CipherText, 0, ct, 0, CipherText.Length);
                Buffer.BlockCopy(Tag, 0, ct, CipherText.Length, Tag.Length);

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(secretKeySpec), Tag.Length * 8, Iv);
                cipher.Init(false, parameters);

                var decrypted = new byte[cipher.GetOutputSize(ct.Length)];
                var len = cipher.ProcessBytes(ct, 0, ct.Length, decrypted, 0);
                cipher.DoFinal(decrypted, len);
                return Encoding.UTF8.GetString(decrypted);
            }

        }

        public override T Accept<T>(EncryptedValueVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
