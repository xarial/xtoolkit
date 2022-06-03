namespace Xarial.XToolkit.Services.Expressions
{
    public interface IExpressionElementGroup : IExpressionElement 
    {
        IExpressionElement[] Children { get; }
    }

    public class ExpressionElementGroup : IExpressionElementGroup
    {
        public IExpressionElement[] Children { get; }

        public ExpressionElementGroup(IExpressionElement[] children)
        {
            Children = children;
        }
    }
}
