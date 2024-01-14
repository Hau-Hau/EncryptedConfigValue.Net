using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using McMaster.Extensions.CommandLineUtils;

namespace EncryptedConfigValue.Cli
{
    internal sealed class GenerateKeyCommand : CommandLineApplication
    {
        public GenerateKeyCommand() : base()
        {
            HelpOption("-?|-h|--help");
            Name = "generate-random-key";
            Description = "Generates a random key for encrypting config values";
            var algorithmOption = Option<string>("-a|--algorithm <ALGORITHM>", "The algorithm to use", CommandOptionType.SingleValue, _ => { }, false);
            algorithmOption.IsRequired(false);
            algorithmOption.Accepts(x => x.Values(Algorithm.AES.ToString(), Algorithm.RSA.ToString()));

            var fileOption = Option<string?>("-f|--file <FILE>", "The location to write the key", CommandOptionType.SingleValue, _ => { }, false);
            fileOption.IsRequired(false);
            fileOption.DefaultValue = KeyFileUtils.DefaultPublicKeyPath;
            OnExecute(() =>
            {
                Run(algorithmOption.Value()!, fileOption.Value());
                return 0;
            });
        }

        private static void Run(string algorithmType, string? path)
        {
            var algorithm = Algorithm.FromString(algorithmType);
            KeyPair keyPair = algorithm.NewKeyPair();

            KeyPairFiles keyPairFiles = KeyFileUtils.KeyPairToFile(keyPair, path);

            // print to console, notifying that we did something
            Console.WriteLine($"Wrote key to {path}");
            if (!keyPairFiles.PathsEqual())
            {
                Console.WriteLine("Wrote private key to " + keyPairFiles.DecryptionKeyFile);
            }
        }
    }
}
