//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
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
    internal class ReadSettingsJsonConverter : SettingsJsonConverter
    {
        private IEnumerable<VersionTransform> m_Transformers;

        internal ReadSettingsJsonConverter(Type settsType, IEnumerable<VersionTransform> transformers, Version latestVers)
            : base(settsType, true, false, latestVers)
        {
            m_Transformers = transformers;
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);
            
            var versToken = jToken.SelectToken(VERSION_NODE_NAME);

            Version settsVers;

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

            return jToken.ToObject(objectType, GetSerializer(serializer));
        }
    }
}
