//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Services;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class ExpressionVariableLinkCollection : Collection<IExpressionVariableLink> 
    {
    }

    public interface IExpressionVariableLink 
    {
        string Title { get; }
        string Description { get; }
        ImageSource Icon { get; }
        bool EnterArguments { get; }
        IExpressionToken CreateExpression(ExpressionBox sender);
    }

    public delegate IExpressionToken ExpressionVariableFactoryDelegate(ExpressionBox sender);

    public class ExpressionVariableLink : IExpressionVariableLink
    {
        public string Title { get; }
        public string Description { get; }
        public ImageSource Icon { get; }
        public bool EnterArguments { get; }

        private readonly ExpressionVariableFactoryDelegate m_Factory;

        public ExpressionVariableLink(string title, string description, ImageSource icon, ExpressionVariableFactoryDelegate factory, bool enterArgs) 
        {
            Title = title;
            Description = description;
            Icon = icon;
            m_Factory = factory;
            EnterArguments = enterArgs;
        }

        public IExpressionToken CreateExpression(ExpressionBox sender) => m_Factory.Invoke(sender);
    }

    public class ExpressionVariableLinkGeneric : IExpressionVariableLink
    {
        private readonly UserInputService m_InputBox;

        public ExpressionVariableLinkGeneric(string title, string description, bool enterArgs, string inputTitle, string inputPrompt)
        {
            Title = title;
            Description = description;
            EnterArguments = enterArgs;
            
            InputTitle = inputTitle;
            InputPrompt = inputPrompt;

            m_InputBox = new UserInputService(InputTitle);
        }

        public ExpressionVariableLinkGeneric() : this("Insert New Variable...", "Insert a new variable with the specified name", true, "ExpressionBox", "Variable Name")
        {
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
        public bool EnterArguments { get; set; }

        public string InputTitle { get; set; }
        public string InputPrompt { get; set; }

        public IExpressionToken CreateExpression(ExpressionBox sender) 
        {
            var varName = "";

            if (m_InputBox.TryGetInputAtCursor(InputPrompt, ref varName))
            {
                return new ExpressionTokenVariable(varName, null);
            }
            else
            {
                return null;
            }
        }
    }

    public class NamedExpressionVariableLink  : IExpressionVariableLink
    {
        public string VariableName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
        public bool EnterArguments { get; set; }

        public ExpressionVariableFactoryDelegate Factory { get; }

        public IExpressionToken CreateExpression(ExpressionBox sender)
            => new ExpressionTokenVariable(VariableName, null);
    }
}
