using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace EncryptedConfigValue.Cli.Test
{
    [Collection("Sequential")]
    public class GenerateKeyCommandTest
    {
        private static readonly GenerateKeyCommand command = new GenerateKeyCommand();

        [Theory]
        [MemberData(nameof(Data))]
        public void WeGenerateAValidKey(Algorithm algorithm)
        {
            var version = Environment.Version;
            var targetFrameworkVersion = $"{version.Major}.{version.Minor}";
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"temp-key-directory", targetFrameworkVersion);
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

            command.Execute("--algorithm", algorithm.ToString(), "--file", tempFilePath);

            var keyPair = KeyFileUtils.KeyPairFromPath(tempFilePath);
            keyPair.EncryptionKey.Type.Algorithm.ShouldBeEquivalentTo(algorithm);
        }

        [Fact]
        public void WeDoNotOverwriteAnExistingKeyfile()
        {
            var version = Environment.Version;
            var targetFrameworkVersion = $"{version.Major}.{version.Minor}";
            var tempDirectory = Path.Combine(Path.GetTempPath(), "temp-key-directory", targetFrameworkVersion);
            var tempFilePath = Path.Combine(tempDirectory, "test.key");
            var algorithm = "AES";

            Directory.CreateDirectory(tempDirectory);
            // create the file
            using (File.Create(tempFilePath)) { }

            var act = () =>
            {
                command.Execute("--algorithm", algorithm.ToString(), "--file", tempFilePath);
            };
            var ex = Should.Throw<IOException>(act);
            ex.Message.ShouldMatch("(.*)already exists*");
        }

        public static IEnumerable<object[]> Data() =>
            new[] {
                new object[] { Algorithm.AES },
                new object[] { Algorithm.RSA },
            };
    }
}
