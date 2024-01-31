//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Represents the variable expression token
    /// </summary>
    public interface IExpressionTokenVariable : IExpressionToken
    {
        /// <summary>
        /// Name of the variable
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Arguments of the variable
        /// </summary>
        IExpressionToken[] Arguments { get; }
    }

    /// <summary>
    /// Represents the custom variable whose name can be changed by the user on insertion
    /// </summary>
    public interface IExpressionTokenCustomVariable : IExpressionTokenVariable 
    {
        /// <summary>
        /// Caption for the name
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Description of the name
        /// </summary>
        string Description { get; }
    }

    [DebuggerDisplay("Variable: '{" + nameof(Name) + "}'")]
    public class ExpressionTokenVariable : IExpressionTokenVariable
    {
        public string Name { get; }
        public IExpressionToken[] Arguments { get; }

        public ExpressionTokenVariable(string name, IExpressionToken[] args)
        {
            if (string.IsNullOrEmpty(name)) 
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Arguments = args;
        }
    }

    [DebuggerDisplay("Custom Variable: '{" + nameof(Name) + "}'")]
    public class ExpressionTokenCustomVariable : ExpressionTokenVariable, IExpressionTokenCustomVariable
    {
        public string Caption { get; }
        public string Description { get; }

        public ExpressionTokenCustomVariable(string name, IExpressionToken[] args, string caption, string desc) : base(name, args)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Caption = caption;
            Description = desc;
        }
    }
}
