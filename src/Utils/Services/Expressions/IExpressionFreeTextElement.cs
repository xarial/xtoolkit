using System.Diagnostics;

namespace Xarial.XToolkit.Services.Expressions
{
    public interface IExpressionFreeTextElement : IExpressionElement
    {
        string Text { get; }
    }

    [DebuggerDisplay("Free Text: '{" + nameof(Text) + "}'")]
    public class ExpressionFreeTextElement : IExpressionFreeTextElement
    {
        public string Text { get; }

        public ExpressionFreeTextElement(string text)
        {
            Text = text;
        }
    }
}
