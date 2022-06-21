//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.UserSettings
{
    public interface IValueSerializer
    {
        Type Type { get; }
        string SerializeValue(object val);
        object DeserializeValue(string val);
    }

    public class BaseValueSerializer<TType> : IValueSerializer
    {
        private readonly Func<TType, string> m_Serializer;
        private readonly Func<string, TType> m_Deserializer;

        public BaseValueSerializer() 
        {
        }

        public BaseValueSerializer(Func<TType, string> serializer, Func<string, TType> deserializer)
        {
            m_Serializer = serializer;
            m_Deserializer = deserializer;
        }

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

        Type IValueSerializer.Type => typeof(TType);
        object IValueSerializer.DeserializeValue(string val) => DeserializeValue(val);
        string IValueSerializer.SerializeValue(object val) => SerializeValue((TType)val);
    }
}
