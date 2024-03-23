using EncryptedConfigValue.Crypto;
using FluentAssertions;
using System.Text.Json;
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
            var serialized = JsonSerializer.Serialize(
                kwt,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var expectedSerialization = string.Format("\"{0}\"", kwtString);
            serialized.Should().BeEquivalentTo(expectedSerialization);

            var deserialized = JsonSerializer.Deserialize<KeyWithType>(serialized);
            deserialized!.ToString().Should().BeEquivalentTo(kwt.ToString());
        }
    }
}
