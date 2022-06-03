using System;
using System.Diagnostics;

namespace Xarial.XToolkit.Services.Expressions
{
    public interface IExpressionVariableElement : IExpressionElement
    {
        string Name { get; }
        IExpressionElement[] Arguments { get; }
    }

    [DebuggerDisplay("Variable: '{" + nameof(Name) + "}'")]
    public class ExpressionVariableElement : IExpressionVariableElement
    {
        public string Name { get; }
        public IExpressionElement[] Arguments { get; }

        public ExpressionVariableElement(string name, IExpressionElement[] args)
        {
            if (string.IsNullOrEmpty(name)) 
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Contains(" ")) 
            {
                throw new Exception("Variable name cannot contain a space");
            }

            Name = name;
            Arguments = args;
        }
    }
}
