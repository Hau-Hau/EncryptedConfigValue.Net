using EncryptedConfigValue.Crypto.Algorithm.Aes;
using EncryptedConfigValue.Crypto.Algorithm.Rsa;
using System;

namespace EncryptedConfigValue.Crypto
{
    public class EncryptedValueVisitor<T>
    {
        private readonly Func<LegacyEncryptedValue, T> visitLegacyEncryptedValue;
        private readonly Func<EncryptedValue, T> visitAesEncryptedValue;
        private readonly Func<EncryptedValue, T> visitRsaEncryptedValue;

        public EncryptedValueVisitor(Func<LegacyEncryptedValue, T> visitLegacyEncryptedValue,
            Func<EncryptedValue, T> visitAesEncryptedValue,
            Func<EncryptedValue, T> visitRsaEncryptedValue)
        {
            this.visitLegacyEncryptedValue = visitLegacyEncryptedValue;
            this.visitAesEncryptedValue = visitAesEncryptedValue;
            this.visitRsaEncryptedValue = visitRsaEncryptedValue;
        }

        public T Visit(LegacyEncryptedValue legacyEncryptedValue)
        {
            return visitLegacyEncryptedValue(legacyEncryptedValue);
        }

        public T Visit(AesEncryptedValue aesEncryptedValue)
        {
            return visitAesEncryptedValue(aesEncryptedValue);
        }

        public T Visit(RsaEncryptedValue rsaEncryptedValue)
        {
            return visitRsaEncryptedValue(rsaEncryptedValue);
        }
    }
}
