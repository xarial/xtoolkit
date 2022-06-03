using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XToolkit.Services
{
    public interface IExpressionEvaluator
    {
        T Evaluate<T>(string expression);
    }
}
