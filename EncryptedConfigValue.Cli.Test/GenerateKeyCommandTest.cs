using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace EncryptedConfigValue.Cli.Test
{
    [Collection("Sequential")]
    public class GenerateKeyCommandTest
    {
        private static readonly GenerateKeyCommand command = new GenerateKeyCommand();

        [Theory]
        [MemberData(nameof(Data))]
        private void WeGenerateAValidKey(Algorithm algorithm)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"temp-key-directory");
            var tempFilePath = Path.Combine(tempDirectory, "test.key");

            if (Directory.Exists(tempDirectory))
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Array.ForEach(Directory.GetFiles(tempDirectory), x => File.Move(x, Path.Combine(tempDirectory, $"{Guid.NewGuid()}")));
                }
                Array.ForEach(Directory.GetFiles(tempDirectory), File.Delete);
            }
            Directory.CreateDirectory(tempDirectory);

            command.Execute("--algorithm", algorithm.ToString(), "--file", tempFilePath);

            var keyPair = KeyFileUtils.KeyPairFromPath(tempFilePath);
            keyPair.EncryptionKey.Type.Algorithm.Should().BeEquivalentTo(algorithm);
        }

        [Fact]
        public void WeDoNotOverwriteAnExistingKeyfile()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "temp-key-directory");
            var tempFilePath = Path.Combine(tempDirectory, "test.key");
            var algorithm = "AES";

            Directory.CreateDirectory(tempDirectory);
            // create the file
            using (File.Create(tempFilePath)) { }

            var act = () =>
            {
                command.Execute("--algorithm", algorithm.ToString(), "--file", tempFilePath);
            };

            act.Should().Throw<IOException>().WithMessage("*already exists*");
        }

        public static IEnumerable<object[]> Data() =>
            new[] {
                new object[] { Algorithm.AES },
                new object[] { Algorithm.RSA },
            };
    }
}
