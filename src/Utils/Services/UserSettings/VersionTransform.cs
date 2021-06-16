//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;

namespace Xarial.XToolkit.Services.UserSettings
{
    public class VersionTransform
    {
        public Version From { get; private set; }
        public Version To { get; private set; }

        private Func<JToken, JToken> m_Transform;

        public VersionTransform(Version from, Version to, Func<JToken, JToken> transform)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            From = from;
            To = to;
            m_Transform = transform;
        }

        public JToken Transform(JToken input)
        {
            return m_Transform.Invoke(input);
        }
    }
}
