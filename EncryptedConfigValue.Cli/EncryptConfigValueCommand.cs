using EncryptedConfigValue.Crypto;
using EncryptedConfigValue.Crypto.Algorithm;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace EncryptedConfigValue.Cli
{
    internal sealed class EncryptConfigValueCommand : CommandLineApplication
    {
        public EncryptConfigValueCommand() : base()
        {
            HelpOption("-?|-h|--help");
            Name = "encrypt-config-value";
            Description = "Encrypts a configuration value so it can be stored securely";
            var keyfileOption = Option<string>("-k|--keyfile <KEYFILE>", "The location of the (public) key file", CommandOptionType.SingleValue, _ => { }, false);
            keyfileOption.IsRequired(false);
            keyfileOption.DefaultValue = KeyFileUtils.DefaultPublicKeyPath;

            var valueOption = Option("-v|--value <VALUE>", "The value to encrypt", CommandOptionType.SingleValue, _ => { }, false);
            valueOption.OnValidate(context =>
            {
                var validator = new RequiredAttribute();
                if (!valueOption.HasValue())
                {
                    Console.Write("Supply values for the following parameters:\nvalue: ");
                    valueOption.TryParse(Console.ReadLine());
                }

                if (validator.IsValid(valueOption.Value()))
                {
                    return ValidationResult.Success!;
                }

                return new ValidationResult(validator.FormatErrorMessage(valueOption.LongName!));
            });
            OnExecute(() =>
            {
                Run(keyfileOption.Value()!, valueOption.Value()!);
                return 0;
            });
        }

        private static void Run(string keyfile, string value)
        {
            KeyWithType keyWithType = KeyFileUtils.KeyWithTypeFromPath(keyfile);
            Algorithm algorithm = keyWithType.Type.Algorithm;

            EncryptedValue encryptedValue = algorithm.NewEncrypter().Encrypt(keyWithType, value);

            // print the resulting encrypted value to the console
            Console.Write(encryptedValue);
        }
    }
}
