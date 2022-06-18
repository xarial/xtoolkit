using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Delegates;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.XToolkit.Wpf.Controls
{
    public interface IExpressionVariableLink 
    {
        string Title { get; }
        string Description { get; }
        ImageSource Icon { get; }
        ExpressionVariableFactoryDelegate Factory { get; }
        bool EnterArguments { get; }
    }

    public class ExpressionVariableLink : IExpressionVariableLink
    {
        public string Title { get; }
        public string Description { get; }
        public ImageSource Icon { get; }
        public ExpressionVariableFactoryDelegate Factory { get; }
        public bool EnterArguments { get; }

        public ExpressionVariableLink(string title, string description, ImageSource icon, ExpressionVariableFactoryDelegate factory, bool enterArgs) 
        {
            Title = title;
            Description = description;
            Icon = icon;
            Factory = factory;
            EnterArguments = enterArgs;
        }
    }

    public class ExpressionVariableLinkGeneric : IExpressionVariableLink
    {
        public ExpressionVariableFactoryDelegate Factory { get; }

        public ExpressionVariableLinkGeneric(string title, string description, bool enterArgs, string inputTitle, string inputPrompt)
        {
            Title = title;
            Description = description;
            EnterArguments = enterArgs;
            
            InputTitle = inputTitle;
            InputPrompt = inputPrompt;

            Factory = NewVariable;
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

        private IExpressionTokenVariable NewVariable(ExpressionBox sender) 
        {
            if (InputBox.ShowAtCursor(InputTitle, InputPrompt, out string varName))
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
        public NamedExpressionVariableLink()
        {
            Factory = NewVariable;
        }

        public string VariableName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
        public bool EnterArguments { get; set; }

        public ExpressionVariableFactoryDelegate Factory { get; }

        private IExpressionTokenVariable NewVariable(ExpressionBox sender)
            => new ExpressionTokenVariable(VariableName, null);
    }
}
