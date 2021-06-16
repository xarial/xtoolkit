//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.UserSettings.Converters
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
            return m_Serializer.DeserializeValue((string)reader.Value);
        }

        public override bool CanConvert(Type objectType) => m_Serializer.Type.IsAssignableFrom(objectType);
    }
}
