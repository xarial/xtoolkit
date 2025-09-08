//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;

namespace Xarial.XToolkit.Services.UserSettings.Converters
{
    /// <summary>
    /// Service to get the JSON serializer without the specified converter to avoid infinite loop
    /// </summary>
    internal class JsonSerializerExcludeConverterProvider : IDisposable
    {
        internal JsonSerializer Serializer { get; }

        private readonly JsonConverter m_Conv;

        private readonly int? m_SettsConvIndex;

        internal JsonSerializerExcludeConverterProvider(JsonSerializer baseSer, JsonConverter conv)
        {
            m_Conv = conv;

            Serializer = baseSer;

            m_SettsConvIndex = baseSer.Converters?.IndexOf(conv);

            if (m_SettsConvIndex.HasValue && m_SettsConvIndex.Value != -1)
            {
                baseSer.Converters.RemoveAt(m_SettsConvIndex.Value);
            }
        }

        public void Dispose()
        {
            if (m_SettsConvIndex.HasValue && m_SettsConvIndex.Value != -1)
            {
                Serializer.Converters.Insert(m_SettsConvIndex.Value, m_Conv);
            }
        }
    }
}
