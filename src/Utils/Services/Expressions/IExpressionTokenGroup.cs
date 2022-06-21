//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Represents a group of <see cref="IExpressionToken"/>
    /// </summary>
    public interface IExpressionTokenGroup : IExpressionToken 
    {
        IExpressionToken[] Children { get; }
    }

    public class ExpressionTokenGroup : IExpressionTokenGroup
    {
        public IExpressionToken[] Children { get; }

        public ExpressionTokenGroup(IExpressionToken[] children)
        {
            Children = children;
        }
    }
}
