using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Services.Data.Attributes
{
    /// <summary>
    /// Defines how the enum should be serialized in the <see cref="IDataSerializer"/>
    /// </summary>
    public enum EnumSerializationType_e 
    {
        /// <summary>
        /// Serialized as integer
        /// </summary>
        Number,

        /// <summary>
        /// Serialized as string
        /// </summary>
        Text
    }

    /// <summary>
    /// Defines the data formatting option in the <see cref="IDataSerializer"/>
    /// </summary>
    public enum DataFormatting_e
    {
        /// <summary>
        /// Default formatting
        /// </summary>
        Default,

        /// <summary>
        /// Indented formatting
        /// </summary>
        Indented
    }

    /// <summary>
    /// Options of <see cref="IDataSerializer"/>
    /// </summary>
    /// <remarks>Decorate data class with this attribute to define options</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataSerializerOptionsAttribute : Attribute
    {
        /// <summary>
        /// Defines how the enum should be serialized
        /// </summary>
        public EnumSerializationType_e EnumSerialization { get; set; }

        /// <summary>
        /// Data formatting
        /// </summary>
        public DataFormatting_e Formatting { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumSerialization">Defines how the enum should be serialized</param>
        /// <param name="formatting">Data formatting</param>
        public DataSerializerOptionsAttribute(EnumSerializationType_e enumSerialization, DataFormatting_e formatting) 
        {
            EnumSerialization = enumSerialization;
            Formatting = formatting;
        }
    }
}
