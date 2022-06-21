//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Xarial.XToolkit.Services.UserSettings.Converters
{
    internal class WriteSettingsJsonConverter : SettingsJsonConverter
    {
        internal WriteSettingsJsonConverter(Type settsType, Version latestVers) 
            : base(settsType, false, true, latestVers)
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jObject = JObject.FromObject(value, GetSerializer(serializer));

            jObject.Add(VERSION_NODE_NAME, m_LatestVersion.ToString());

            jObject.WriteTo(writer);
        }
    }
}
