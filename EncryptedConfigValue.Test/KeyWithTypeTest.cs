using EncryptedConfigValue.Crypto;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EncryptedConfigValue.Test
{
    public class KeyWithTypeTest
    {
        private static readonly string kwtString = "AES:rqrvWpLld+wKLOyxJYxQVg==";
        private static readonly KeyWithType kwt = KeyWithType.FromString(kwtString);

        [Fact]
        public void TestSerialization()
        {
            var serialized = JsonConvert.SerializeObject(kwt);

            var expectedSerialization = string.Format("\"{0}\"", kwtString);
            serialized.Should().BeEquivalentTo(expectedSerialization);

            var deserialized = JsonConvert.DeserializeObject<KeyWithType>(serialized);
            deserialized!.ToString().Should().BeEquivalentTo(kwt.ToString());
        }
    }
}
