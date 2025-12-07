using EncryptedConfigValue.AspNetCore.Test.Util;
using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Util;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Shouldly;
using System.Text.RegularExpressions;

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

            configuration["Unencrypted"].ShouldBe("value");
            configuration["Encrypted"].ShouldBe("value");
            configuration["EncryptedWithSingleQuote"].ShouldBe("don't use quotes");
            configuration["EncryptedWithDoubleQuote"].ShouldBe("double quote is \"");
            configuration["EncryptedMalformedYaml"].ShouldBe("[oh dear");

            configuration
                .GetSection("ArrayWithSomeEncryptedValues")
                .GetChildren()
                .Select(x => x.Value)
                .ToList()
                .ShouldBeEquivalentTo(new List<string> { "value", "value", "other value", "[oh dear" });

            var person = new Person();
            configuration
                .GetSection("PocoWithEncryptedValues")
                .Bind(person);
            person.Username.ShouldBeEquivalentTo("some-user");
            person.Password.ShouldBeEquivalentTo("value");
        }

        [Fact]
        public void TestDecryptionFailsWithNiceMessage()
        {
            var act = () => new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("Resources", "testConfigWithError.json"), optional: false, reloadOnChange: false)
                .AddEncryptedConfigValueProvider()
                .Build();
            var ex = Should.Throw<ConfigurationDecryptionException>(act);
            ex.Message.ShouldMatch($@"Configuration decryption error at {Regex.Escape(Path.Combine("Resources", "testConfigWithError.json"))} \(.*\)");
            ex.InnerException.ShouldBeOfType<StringSubstitutionException>();
            ex.InnerException.Message.ShouldBe("The value 'enc:ERROR' for field 'ArrayWithSomeEncryptedValues:3' could not be replaced");
        }
    }
}
