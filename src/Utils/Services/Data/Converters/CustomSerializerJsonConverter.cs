//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xarial.XToolkit.Services.Data.Converters
{
    internal class CustomSerializerJsonConverter : JsonConverter
    {
        private readonly IValueSerializer m_Serializer;

        internal CustomSerializerJsonConverter(IValueSerializer serializer) 
        {
            m_Serializer = serializer;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(m_Serializer.SerializeValue(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value;

            if (IsValue(reader.TokenType))
            {
                value = reader.Value?.ToString();
            }
            else
            {
                var jToken = JToken.Load(reader);

                var content = new StringBuilder();

                using (var stringWriter = new StringWriter(content))
                {
                    using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                    {
                        jToken.WriteTo(jsonTextWriter);
                    }
                }

                value = content.ToString();
            }

            return m_Serializer.DeserializeValue(value);
        }

        public override bool CanConvert(Type objectType) => m_Serializer.Type.IsAssignableFrom(objectType);

        private bool IsValue(JsonToken token) =>
            token == JsonToken.String ||
            token == JsonToken.Integer ||
            token == JsonToken.Float ||
            token == JsonToken.Boolean ||
            token == JsonToken.Null ||
            token == JsonToken.Date ||
            token == JsonToken.Bytes;
    }
}
