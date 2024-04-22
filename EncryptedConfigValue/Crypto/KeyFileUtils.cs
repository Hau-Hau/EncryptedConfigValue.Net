using System;
using System.IO;
using System.Text;

namespace EncryptedConfigValue.Crypto
{
    public static class KeyFileUtils
    {
        // KeyPathProperty is different than in original Java implementation.
        public const string KeyPathProperty = "encrypted_config_value.config.key_path";
        public const string DefaultPublicKeyPath = "var/conf/encrypted-config-value.key";

        public static string DecryptUsingDefaultKeys(EncryptedValue encryptedValue)
        {
            KeyPair keyPair;
            try
            {
                keyPair = KeyPairFromDefaultPath();
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed to read key", e);
            }

            return encryptedValue.Decrypt(keyPair.DecryptionKey);
        }

        public static KeyWithType KeyWithTypeFromPath(string keyPath)
        {
            var contents = File.ReadAllBytes(keyPath);
            return KeyWithType.FromString(Encoding.UTF8.GetString(contents));
        }

        public static void KeyWithTypeToFile(KeyWithType kwt, string path)
        {
            byte[] serialized = Encoding.UTF8.GetBytes(kwt.ToString());
            using (FileStream fileStream = new FileStream(path, FileMode.CreateNew))
            {
                fileStream.Write(serialized, 0, serialized.Length);
            }
        }

        public static KeyPairFiles KeyPairToFile(KeyPair keyPair, string path)
        {
            KeyWithTypeToFile(keyPair.EncryptionKey, path);

            string decryptionKeyPath = path;
            if (keyPair.EncryptionKey != keyPair.DecryptionKey)
            {
                decryptionKeyPath = PrivatePath(path);
                KeyWithTypeToFile(keyPair.DecryptionKey, decryptionKeyPath);
            }

            return new KeyPairFiles(path, decryptionKeyPath);
        }

        public static KeyPair KeyPairFromPath(string path)
        {
            KeyWithType encryptionKey = KeyWithTypeFromPath(path);
            var privatePath = PrivatePath(path);
            if (!File.Exists(privatePath))
            {
                return KeyPair.Symmetric(encryptionKey);
            }
            KeyWithType decryptionKey = KeyWithTypeFromPath(privatePath);
            return KeyPair.Of(encryptionKey, decryptionKey);
        }

        public static KeyPair KeyPairFromDefaultPath()
        {
            return KeyPairFromPath(Environment.GetEnvironmentVariable(KeyPathProperty) ?? DefaultPublicKeyPath);
        }

        // Returns the sibling path of the provided path with ".private" as the extension.
        private static string PrivatePath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}.private");
        }
    }
}