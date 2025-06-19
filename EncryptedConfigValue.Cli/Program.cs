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
            try
            {
                await app.ExecuteAsync(args);
            }
            catch (CommandParsingException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }
    }
}
