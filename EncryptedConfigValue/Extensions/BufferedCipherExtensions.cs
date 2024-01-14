using Org.BouncyCastle.Crypto.Encodings;
using System.Collections.Generic;
using System.IO;

namespace EncryptedConfigValue.Extensions
{
    public static class BufferedCipherExtensions
    {
        public static byte[] ApplyCipher(this OaepEncoding that, byte[] data, int blockSize = 10)
        {
            var inputStream = new MemoryStream(data);
            var outputBytes = new List<byte>();
            var buffer = new byte[blockSize];
            int index;
            while ((index = inputStream.Read(buffer, 0, blockSize)) > 0)
            {
                var cipherBlock = that.ProcessBlock(buffer, 0, index);
                outputBytes.AddRange(cipherBlock);
            }
            return outputBytes.ToArray();
        }
    }
}