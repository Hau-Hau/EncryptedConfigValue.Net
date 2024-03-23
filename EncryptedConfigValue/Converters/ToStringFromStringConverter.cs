using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EncryptedConfigValue.Converters
{
    internal sealed class ToStringFromStringConverter<T> : JsonConverter<T>
    {
        private const string FromStringMethodName = "FromString";
        private static readonly MethodInfo FromStringMethod;

        static ToStringFromStringConverter()
        {
            FromStringMethod = typeof(T).GetMethod(FromStringMethodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (T)FromStringMethod.Invoke(null, new object[] { reader.GetString()});
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteRawValue($"\"{value}\"");
        }
    }
}
