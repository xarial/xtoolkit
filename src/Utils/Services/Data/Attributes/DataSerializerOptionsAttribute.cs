//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
    /// Null object value handling option in the <see cref="IDataSerializer"/>
    /// </summary>
    public enum NullValueHandling_e
    {
        /// <summary>
        /// Include null objects
        /// </summary>
        Include,
        
        /// <summary>
        /// Ignore null objects
        /// </summary>
        Ignore
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
        /// Null object value handling
        /// </summary>
        public NullValueHandling_e NullValueHandling { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataSerializerOptionsAttribute() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enumSerialization">Defines how the enum should be serialized</param>
        /// <param name="formatting">Data formatting</param>
        /// <param name="nullValHandling">Null object value handling</param>
        public DataSerializerOptionsAttribute(EnumSerializationType_e enumSerialization, DataFormatting_e formatting, NullValueHandling_e nullValHandling) 
        {
            EnumSerialization = enumSerialization;
            Formatting = formatting;
            NullValueHandling = nullValHandling;
        }
    }
}
