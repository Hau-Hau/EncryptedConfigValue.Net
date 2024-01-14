using McMaster.Extensions.CommandLineUtils;

namespace EncryptedConfigValue.Cli
{
    internal sealed class Program
    {
        static async Task Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption("-?|-h|--help");
            app.AddSubcommand(new GenerateKeyCommand());
            app.AddSubcommand(new EncryptConfigValueCommand());
            await app.ExecuteAsync(args);
        }
    }
}
