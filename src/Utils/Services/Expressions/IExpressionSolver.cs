using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services.Expressions
{
    public interface IExpressionSolver
    {
        T Solve<T>(IExpressionElement[] elements);
    }
}
