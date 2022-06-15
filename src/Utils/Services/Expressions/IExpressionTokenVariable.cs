using System;
using System.Diagnostics;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Represents the variable expression token
    /// </summary>
    public interface IExpressionTokenVariable : IExpressionToken
    {
        string Name { get; }
        IExpressionToken[] Arguments { get; }
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
}
