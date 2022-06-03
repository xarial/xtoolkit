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
            Name = name;
            Arguments = args;
        }
    }
}
