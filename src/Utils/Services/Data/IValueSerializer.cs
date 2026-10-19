//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Data
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
        /// <param name="val">Data</param>
        /// <returns>JSON-value</returns>
        object SerializeValue(object val);

        /// <summary>
        /// Deserialize value
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Deserialized value</returns>
        object DeserializeValue(object val);
    }

    /// <summary>
    /// Base type safe <see cref="IValueSerializer"/>
    /// </summary>
    /// <typeparam name="TFrom">Specific object type</typeparam>
    /// <typeparam name="TTo">Specific serialization value type</typeparam>
    public class ValueSerializer<TFrom, TTo> : IValueSerializer
    {
        Type IValueSerializer.Type => typeof(TFrom);
        object IValueSerializer.DeserializeValue(object val) => DeserializeValue((TTo)val);
        object IValueSerializer.SerializeValue(object val) => SerializeValue((TFrom)val);

        private readonly Func<TFrom, TTo> m_Serializer;
        private readonly Func<TTo, TFrom> m_Deserializer;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ValueSerializer() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializer">Serializer function</param>
        /// <param name="deserializer">Deserializer function</param>
        public ValueSerializer(Func<TFrom, TTo> serializer, Func<TTo, TFrom> deserializer)
        {
            m_Serializer = serializer;
            m_Deserializer = deserializer;
        }

        /// <inheritdoc/>
        protected virtual TFrom DeserializeValue(TTo val) 
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
        protected virtual TTo SerializeValue(TFrom val)
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
