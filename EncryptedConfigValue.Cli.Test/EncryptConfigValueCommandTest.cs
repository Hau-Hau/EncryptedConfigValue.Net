using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace EncryptedConfigValue.Cli.Test
{
    [Collection("Sequential")]
    public class EncryptConfigValueCommandTest : IDisposable
    {
        private sealed class StringWriterWithEncoding(Encoding encoding) : StringWriter()
        {
            public override Encoding Encoding { get; } = encoding;
        }

        private static readonly string CHARSET = "UTF-8";
        private static readonly string plaintext = "this is a secret message";

        private readonly StringWriter outContent = new StringWriterWithEncoding(Encoding.GetEncoding(CHARSET));
        private readonly EncryptConfigValueCommand command = new();

        private TextWriter originalConsoleOut;
        public EncryptConfigValueCommandTest()
        {
            originalConsoleOut = Console.Out;
            Console.SetOut(outContent);
        }

        public void Dispose()
        {
            Console.SetOut(originalConsoleOut);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void WeEncryptAndPrintAValue(Algorithm algorithm)
        {
            var version = Environment.Version;
            var targetFrameworkVersion = $"{version.Major}.{version.Minor}";
            var tempDirectory = Path.Combine(Path.GetTempPath(), "temp-key-directory", targetFrameworkVersion);
            var tempFilePath = Path.Combine(tempDirectory, "test.key");

            if (Directory.Exists(tempDirectory))
            {
                Directory
                    .GetFiles(tempDirectory)
                    .Select(file =>
                    {
                        var newPath = Path.Combine(tempDirectory, Path.GetFileName(file) + ".deleted");
                        File.Move(file, newPath);
                        return newPath;
                    })
                    .ToList()
                    .ForEach(File.Delete);
            }
            Directory.CreateDirectory(tempDirectory);

            var keyPair = algorithm.NewKeyPair();
            KeyFileUtils.KeyPairToFile(keyPair, tempFilePath);

            command.Execute("--keyfile", tempFilePath, "--value", plaintext);
            var output = outContent.ToString();
            var configValue = EncryptedValue.FromString(output);
            var decryptionKey = keyPair.DecryptionKey;
            var decryptedValue = configValue.Decrypt(decryptionKey);

            decryptedValue.Should().BeEquivalentTo(plaintext);
        }

        [Fact]
        internal void WeFailIfTheKeyfileDoesNotExist()
        {
            var version = Environment.Version;
            var targetFrameworkVersion = $"{version.Major}.{version.Minor}";
            var tempDirectory = Path.Combine(Path.GetTempPath(), "temp-key-directory", targetFrameworkVersion);
            var tempFilePath = Path.Combine(tempDirectory, "test.key");

            if (Directory.Exists(tempDirectory))
            {
                Directory
                    .GetFiles(tempDirectory)
                    .Select(file =>
                    {
                        var newPath = Path.Combine(tempDirectory, Path.GetFileName(file) + ".deleted");
                        File.Move(file, newPath);
                        return newPath;
                    })
                    .ToList()
                    .ForEach(File.Delete);
            }
            Directory.CreateDirectory(tempDirectory);

            var act = () =>
            {
                command.Execute("--keyfile", tempFilePath, "--value", plaintext);
            };

            act.Should().Throw<FileNotFoundException>();
        }

        public static IEnumerable<object[]> Data() =>
            new[] {
                new object[] { Algorithm.AES },
                new object[] { Algorithm.RSA },
            };
    }
}
