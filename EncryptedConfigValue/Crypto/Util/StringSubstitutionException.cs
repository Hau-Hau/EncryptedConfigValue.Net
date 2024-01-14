using System;
using System.Text;

namespace EncryptedConfigValue.Crypto.Util
{
    public class StringSubstitutionException : Exception
    {
        private readonly StringBuilder fieldBuilder;
        private readonly bool lastExtensionWasArrayIndex;

        public override string Message => $"The value '{Value}' for field '{Field}' could not be replaced";

        private StringSubstitutionException(
            Exception cause, string value, StringBuilder fieldBuilder, bool lastExtensionWasArrayIndex)
            : base(null, cause)
        {
            Value = value;
            this.fieldBuilder = fieldBuilder;
            this.lastExtensionWasArrayIndex = lastExtensionWasArrayIndex;
        }

        public StringSubstitutionException(Exception cause, string value)
            : this(cause, value, new StringBuilder(), false)
        {
        }

        public StringSubstitutionException Extend(string field)
        {
            return Extend(field, false);
        }

        public StringSubstitutionException Extend(int arrayIndex)
        {
            return Extend("[" + arrayIndex + "]", true);
        }

        private StringSubstitutionException Extend(string prefix, bool prefixIsArrayIndex)
        {
            var extendedFieldBuilder = new StringBuilder(prefix);
            if (fieldBuilder.Length > 0 && !lastExtensionWasArrayIndex)
            {
                extendedFieldBuilder.Append(".");
            }
            extendedFieldBuilder.Append(fieldBuilder);
            return new StringSubstitutionException(
                InnerException, Value, extendedFieldBuilder, prefixIsArrayIndex);
        }

        public string Field => fieldBuilder.ToString();

        public string Value { get; }

        public Exception Cause => InnerException;
    }
}
