//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.UserSettings
{
    /// <summary>
    /// Custom value serializer in <see cref="IVersionsTransformer"/>
    /// </summary>
    public interface IValueSerializer
    {
        /// <summary>
        /// Type to serialize
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Serialize value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        string SerializeValue(object val);

        /// <summary>
        /// Deserialize value
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        object DeserializeValue(string val);
    }

    /// <summary>
    /// Base type safe <see cref="IValueSerializer"/>
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class BaseValueSerializer<TType> : IValueSerializer
    {
        Type IValueSerializer.Type => typeof(TType);
        object IValueSerializer.DeserializeValue(string val) => DeserializeValue(val);
        string IValueSerializer.SerializeValue(object val) => SerializeValue((TType)val);

        private readonly Func<TType, string> m_Serializer;
        private readonly Func<string, TType> m_Deserializer;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseValueSerializer() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializer">Serializer function</param>
        /// <param name="deserializer">Deserializer function</param>
        public BaseValueSerializer(Func<TType, string> serializer, Func<string, TType> deserializer)
        {
            m_Serializer = serializer;
            m_Deserializer = deserializer;
        }

        /// <inheritdoc/>
        protected virtual TType DeserializeValue(string val) 
        {
            if (m_Deserializer != null)
            {
                return m_Deserializer.Invoke(val);
            }
            else 
            {
                throw new Exception($"Either override the {nameof(DeserializeValue)} or provide a deserializer delegate in the constructor");
            }
        }

        /// <inheritdoc/>
        protected virtual string SerializeValue(TType val)
        {
            if (m_Serializer != null)
            {
                return m_Serializer.Invoke(val);
            }
            else
            {
                throw new Exception($"Either override the {nameof(SerializeValue)} or provide a serializer delegate in the constructor");
            }
        }
    }
}
