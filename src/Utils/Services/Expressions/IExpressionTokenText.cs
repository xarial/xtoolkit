//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System.Diagnostics;

namespace Xarial.XToolkit.Services.Expressions
{
    /// <summary>
    /// Represents the free text expression token
    /// </summary>
    public interface IExpressionTokenText : IExpressionToken
    {
        string Text { get; }
    }

    [DebuggerDisplay("Text: '{" + nameof(Text) + "}'")]
    public class ExpressionTokenText : IExpressionTokenText
    {
        public string Text { get; }

        public ExpressionTokenText(string text)
        {
            Text = text;
        }
    }
}
