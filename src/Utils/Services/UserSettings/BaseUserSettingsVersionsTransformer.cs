//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Xarial.XToolkit.Services.UserSettings
{
    public class BaseUserSettingsVersionsTransformer : IEnumerable<VersionTransform>
    {
        private List<VersionTransform> m_Transformers;

        public BaseUserSettingsVersionsTransformer(params VersionTransform[] transformers)
        {
            m_Transformers = new List<VersionTransform>();

            if (transformers != null)
            {
                m_Transformers.AddRange(transformers);
            }
        }

        public IEnumerator<VersionTransform> GetEnumerator() => m_Transformers.GetEnumerator();

        protected void Add(Version from, Version to, Func<JToken, JToken> transform)
        {
            m_Transformers.Add(new VersionTransform(from, to, transform));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
