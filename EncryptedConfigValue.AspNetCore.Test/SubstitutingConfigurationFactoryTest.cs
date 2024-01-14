using EncryptedConfigValue.AspNetCore.Test.Util;
using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Util;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace EncryptedConfigValue.AspNetCore.Test
{
    public class SubstitutingConfigurationFactoryTest
    {
        static SubstitutingConfigurationFactoryTest()
        {
            System.Environment.SetEnvironmentVariable(KeyFileUtils.KeyPathProperty, Path.Combine("Resources", "test.key"));
        }

        [Fact]
        public void TestDecryptionSucceeds()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("Resources", "testConfig.json"), optional: false, reloadOnChange: false)
                .AddEncryptedConfigValueProvider()
                .Build();

            configuration["Unencrypted"].Should().Be("value");
            configuration["Encrypted"].Should().Be("value");
            configuration["EncryptedWithSingleQuote"].Should().Be("don't use quotes");
            configuration["EncryptedWithDoubleQuote"].Should().Be("double quote is \"");
            configuration["EncryptedMalformedYaml"].Should().Be("[oh dear");

            configuration
                .GetSection("ArrayWithSomeEncryptedValues")
                .GetChildren()
                .Select(x => x.Value)
                .Should()
                .BeEquivalentTo(new List<string> { "value", "value", "other value", "[oh dear" });

            var person = new Person();
            configuration
                .GetSection("PocoWithEncryptedValues")
                .Bind(person);
            person.Username.Should().BeEquivalentTo("some-user");
            person.Password.Should().BeEquivalentTo("value");
        }

        [Fact]
        public void TestDecryptionFailsWithNiceMessage()
        {
            var act = () => new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("Resources", "testConfigWithError.json"), optional: false, reloadOnChange: false)
                .AddEncryptedConfigValueProvider()
                .Build();
            act.Should()
                .Throw<ConfigurationDecryptionException>()
                .WithMessage($"Configuration decryption error at {Path.Combine("Resources", "testConfigWithError.json")} (*)")
                .WithInnerExceptionExactly<StringSubstitutionException>()
                .WithMessage("The value 'enc:ERROR' for field 'arrayWithSomeEncryptedValues:3' could not be replaced");
        }
    }
}
