using Newtonsoft.Json;
using System;
using System.Reflection;

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

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (T)FromStringMethod.Invoke(null, new object[] { reader.Value as string });
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"\"{value}\"");
        }
    }
}
