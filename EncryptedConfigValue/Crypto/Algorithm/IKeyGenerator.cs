namespace EncryptedConfigValue.Crypto.Algorithm
{
    public interface IKeyGenerator
    {
        /// <summary>
        /// Creates a <see cref="KeyWithType"/> based on the provided key content bytes. The provided bytes should correspond to
        /// the bytes returned by <see cref="IKey.Bytes"/>. The type of the key is supplied by the implementor of the interface.
        /// </summary>
        KeyWithType KeyFromBytes(byte[] key);
    }
}