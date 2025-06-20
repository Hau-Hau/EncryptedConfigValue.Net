using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using EncryptedConfigValue.Crypto.Util;
using EncryptedConfigValue.Module;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace EncryptedConfigValue.AspNetCore.Test
{
    public class DecryptingVariableSubstitutorTest : IDisposable
    {
        private static readonly Algorithm Algorithm = Algorithm.RSA;
        private static readonly KeyPair KeyPair = Algorithm.NewKeyPair();
        public const string TEST_KEY_PATH = nameof(DecryptingVariableSubstitutorTest) + "test-key";
        private static string? previousProperty;

        private readonly DecryptingVariableSubstitutor substitutor = new DecryptingVariableSubstitutor();

        public DecryptingVariableSubstitutorTest()
        {
            previousProperty = Environment.GetEnvironmentVariable(KeyFileUtils.KeyPathProperty);
            EnsureTestKeysExist();
        }

        public void Dispose()
        {
            if (previousProperty != null)
            {
                Environment.SetEnvironmentVariable(KeyFileUtils.KeyPathProperty, previousProperty);
            }

            var tempKeyPath = Environment.GetEnvironmentVariable(TEST_KEY_PATH);
            var tempKeyPathDir = string.IsNullOrWhiteSpace(tempKeyPath)
                ? null
                : Path.GetDirectoryName(tempKeyPath);
            if (!string.IsNullOrWhiteSpace(tempKeyPathDir))
            {
                if (Directory.Exists(tempKeyPath))
                {
                    Directory
                        .GetFiles(tempKeyPath)
                        .Select(file =>
                        {
                            var newPath = Path.Combine(tempKeyPath, Path.GetFileName(file) + ".deleted");
                            File.Move(file, newPath);
                            return newPath;
                        })
                        .ToList()
                        .ForEach(File.Delete);
                }
            }

            Environment.SetEnvironmentVariable(TEST_KEY_PATH, null);
        }

        [Fact]
        public void ConstantsAreNotModified()
        {
            substitutor.TryReplace("abc", out var output);
            output.Should().BeEquivalentTo("abc");
        }

        [Fact]
        public void InvalidEncryptedVariablesThrowStringSubstitutionException()
        {
            try
            {
                substitutor.TryReplace("${enc:invalid-contents}", out var output);
                Assert.Fail();
            }
            catch (StringSubstitutionException e)
            {
                e.Value.Should().BeEquivalentTo("enc:invalid-contents");
                e.Field.Should().BeEmpty();
            }
        }

        [Fact]
        public void NonEncryptedVariablesAreNotModified()
        {
            substitutor.TryReplace("${abc}", out var output);
            output.Should().BeEquivalentTo("${abc}");
        }

        [Fact]
        public void VariableIsDecrypted()
        {
            substitutor.TryReplace("${" + Encrypt("abc") + "}", out var output);
            output.Should().BeEquivalentTo("abc");
        }

        [Fact]
        public void VariableIsDecryptedWithRegex()
        {
            substitutor.TryReplace("${" + Encrypt("$5") + "}", out var output);
            output.Should().Be("$5");
        }

        [Fact]
        public void DecryptsMultiple()
        {
            var abc = "${" + Encrypt("abc") + "}";
            var def = "${" + Encrypt("def") + "}";
            var hello = "${" + Encrypt("enc:hello") + "}";
            var source = abc + ":" + def + '.' + hello;
            substitutor.TryReplace(source, out var output);
            output.Should().Be("abc:def.enc:hello");
        }

        [Fact]
        public void DecryptsWithPlaceholders()
        {
            var abc = "${" + Encrypt("abc") + "}";
            var def = "${" + Encrypt("${enc:test}") + "}";
            var source = abc + ":" + def;
            substitutor.TryReplace(source, out var output);
            output.Should().BeEquivalentTo("abc:${enc:test}");
        }

        [SkippableTheory]
        [MemberData(nameof(PropertyTestData), 100)]
        public void PropertyTestValues(string plaintext)
        {
            // RSA test key can only encrypt 190 bytes
            Skip.If(Encoding.UTF8.GetBytes(plaintext).Length > 190);
            EnsureTestKeysExist();
            substitutor.TryReplace("${" + Encrypt(plaintext) + "}", out var output);
            output.Should().BeEquivalentTo(plaintext);
        }

        public static IEnumerable<object[]> PropertyTestData(int tries)
        {
            foreach (var i in Enumerable.Range(start: 0, count: tries))
            {
                yield return new object[] {
                    new string(Enumerable.Repeat(0, RandomNumberGenerator.GetInt32(0, 100))
                        .Select(_ => (char)RandomNumberGenerator.GetInt32(0, 1024))
                        .ToArray())
                };
            }
        }

        private static void EnsureTestKeysExist()
        {
            var testKeyPath = Environment.GetEnvironmentVariable(TEST_KEY_PATH);
            if (testKeyPath != null && File.Exists(testKeyPath))
            {
                return;
            }

            var tempDirectory = Path.Combine(Directory.CreateTempSubdirectory().ToString(), "temp-key-directory");
            Directory.CreateDirectory(tempDirectory);
            var tempFilePath = Path.Combine(tempDirectory, $"{Algorithm}-test.key");
            KeyFileUtils.KeyPairToFile(KeyPair, tempFilePath);
            Environment.SetEnvironmentVariable(KeyFileUtils.KeyPathProperty, tempFilePath);
            Environment.SetEnvironmentVariable(TEST_KEY_PATH, tempFilePath);
        }

        private string Encrypt(string value)
        {
            return Algorithm.NewEncrypter().Encrypt(KeyPair.EncryptionKey, value).ToString();
        }
    }
}
