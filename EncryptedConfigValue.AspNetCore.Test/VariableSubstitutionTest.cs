using EncryptedConfigValue.AspNetCore.Test.Util;
using EncryptedConfigValue.Crypto;
using Shouldly;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace EncryptedConfigValue.AspNetCore.Test
{
    public class VariableSubstitutionTest
    {
        public VariableSubstitutionTest()
        {
            Environment.SetEnvironmentVariable(KeyFileUtils.KeyPathProperty, Path.Combine("Resources", "test.key"));
        }

        private static WebApplication CreateWebApplication()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile(Path.Combine("Resources", "testConfig.json"), optional: false, reloadOnChange: false);
            builder.AddEncryptedConfigValueProvider();
            return builder.Build();
        }

        private static readonly WebApplication aspnet = CreateWebApplication();

        [Fact]
        public void TestCanDecryptValueInConfig()
        {
            aspnet.Configuration["Unencrypted"].ShouldBe("value");
            aspnet.Configuration["Encrypted"].ShouldBe("value");
            aspnet.Configuration["EncryptedWithSingleQuote"].ShouldBe("don't use quotes");
            aspnet.Configuration["EncryptedWithDoubleQuote"].ShouldBe("double quote is \"");
            aspnet.Configuration["EncryptedMalformedYaml"].ShouldBe("[oh dear");

            aspnet.Configuration
                .GetSection("ArrayWithSomeEncryptedValues")
                .GetChildren()
                .Select(x => x.Value)
                .ToList()
                .ShouldBeEquivalentTo(new List<string> { "value", "value", "other value", "[oh dear" });

            var person = new Person();
            aspnet.Configuration
                .GetSection("PocoWithEncryptedValues")
                .Bind(person);
            person.Username.ShouldBeEquivalentTo("some-user");
            person.Password.ShouldBeEquivalentTo("value");
        }
    }
}
