//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xarial.XToolkit.Services.UserSettings.Converters
{
    internal class SettingsJsonConverter : JsonConverter
    {
        protected string VERSION_NODE_NAME = "$version";
        protected string LEGACY_VERSION_NODE_NAME = "__version";

        protected Type m_SettsType;
        protected Version m_LatestVersion;

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => objectType == m_SettsType;

        private IReadOnlyList<VersionTransform> m_Transformers;

        internal SettingsJsonConverter(Type settsType, IReadOnlyList<VersionTransform> transformers, Version latestVers)
        {
            m_SettsType = settsType;
            m_Transformers = transformers;
            m_LatestVersion = latestVers;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            using (var jsonSerProv = new JsonSerializerExcludeConverterProvider(serializer, this))
            {
                var jObject = JObject.FromObject(value, jsonSerProv.Serializer);

                jObject.Add(VERSION_NODE_NAME, m_LatestVersion.ToString());

                jObject.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            Version settsVers;

            var versToken = jToken.SelectToken(VERSION_NODE_NAME);

            if (versToken == null)
            {
                versToken = jToken.SelectToken(LEGACY_VERSION_NODE_NAME);
            }

            if (versToken == null)
            {
                settsVers = new Version();
            }
            else
            {
                settsVers = Version.Parse(versToken.Value<string>());
            }

            if (m_LatestVersion > settsVers)
            {
                if (m_Transformers != null)
                {
                    foreach (var tr in m_Transformers
                        .Where(t => t.From >= settsVers && t.To <= m_LatestVersion)
                        .OrderBy(t => t.From))
                    {
                        jToken = tr.Transform(jToken);
                    }
                }
            }

            using (var jsonSerProv = new JsonSerializerExcludeConverterProvider(serializer, this))
            {
                return jToken.ToObject(objectType, jsonSerProv.Serializer);
            }
        }
    }
}
