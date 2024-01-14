namespace EncryptedConfigValue.Crypto.Algorithm
{
    public interface IEncrypter
    {
        /// <summary>
        /// Creates an <see cref="EncryptedValue"/> that is the result of encrypting the provided plaintext using the provided key.
        /// The returned <see cref="EncryptedValue"/> should contain information about the algorithm and paramters used to generate
        /// the value. Throws an exception if the provided key cannot be used to generate an <see cref="EncryptedValue"/> for the
        /// algorithm used by the <see cref="IEncrypter"/>.
        /// </summary>
        EncryptedValue Encrypt(KeyWithType kwt, string plaintext);
    }
}
