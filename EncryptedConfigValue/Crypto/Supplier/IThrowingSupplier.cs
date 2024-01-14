namespace EncryptedConfigValue.Crypto.Supplier
{
    public interface IThrowingSupplier<T>
    {
        T Get();
    }
}
