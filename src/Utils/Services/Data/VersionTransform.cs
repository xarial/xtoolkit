//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Version transform instruction in <see cref="IVersionsTransformer"/>
    /// </summary>
    public class VersionTransform
    {
        /// <summary>
        /// Version to transform from
        /// </summary>
        /// <remarks>To transform from the unknown version use the new Version()</remarks>
        public Version From { get; }

        /// <summary>
        /// Version to transform to
        /// </summary>
        public Version To { get; }

        private readonly Func<JToken, JToken> m_Transform;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">Version to transform from</param>
        /// <param name="to">Version ot transform to</param>
        /// <param name="transform">Transfomr instruction</param>
        /// <exception cref="ArgumentNullException"></exception>
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

        internal JToken Transform(JToken input) => m_Transform.Invoke(input);
    }
}
