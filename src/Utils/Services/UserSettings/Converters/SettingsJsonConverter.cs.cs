//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;

namespace Xarial.XToolkit.Services.UserSettings.Converters
{
    internal abstract class SettingsJsonConverter : JsonConverter
    {
        protected string VERSION_NODE_NAME = "__version";

        protected Type m_SettsType;
        protected Version m_LatestVersion;

        private bool m_CanRead;
        private bool m_CanWrite;

        protected SettingsJsonConverter(Type settsType, bool canRead, bool canWrite, Version latestVers)
        {
            m_SettsType = settsType;

            m_LatestVersion = latestVers;

            m_CanRead = canRead;
            m_CanWrite = canWrite;
        }

        public override bool CanRead => m_CanRead;

        public override bool CanWrite => m_CanWrite;

        public override bool CanConvert(Type objectType)
        {
            return objectType == m_SettsType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected JsonSerializer GetSerializer(JsonSerializer baseSer)
        {
            if (baseSer.Converters?.Contains(this) == true)
            {
                baseSer.Converters.Remove(this);
            }

            return baseSer;
        }
    }
}
