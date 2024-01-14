namespace EncryptedConfigValue.AspNetCore.Test.Util
{
    internal class Person
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public static Person Of(string username, string password)
        {
            return new Person { Username = username, Password = password };
        }
    }
}
