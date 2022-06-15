using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfTester.Properties;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Extensions;

namespace WpfTester
{
    public enum Options_e 
    {
        [EnumDisplayName("First Item")]
        Item1,
        Item2,
        Item3
    }

    [Flags]
    public enum MultiOptions_e
    {
        [EnumDisplayName("First Item")]
        Item1 = 1,
        Item2 = 2,
        Item3 = 4,
        Item4 = 8
    }

    public class MyExpressionVariableDescriptor : IExpressionVariableDescriptor
    {
        private readonly BitmapImage m_Var1Icon;

        public MyExpressionVariableDescriptor() 
        {
            m_Var1Icon = Resources.icon.ToBitmapImage();
            m_Var1Icon.Freeze();
        }

        public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic)
        {
            if (string.Equals(variable.Name, "var1", StringComparison.CurrentCultureIgnoreCase))
            {
                dynamic = false;

                return new ExpressionVariableArgumentDescriptor[]
                {
                    ExpressionVariableArgumentDescriptor.CreateText("Text Argument", "Sample Text Argument"),
                    ExpressionVariableArgumentDescriptor.CreateNumeric("Numeric Argument", "Numeric Text Argument"),
                    ExpressionVariableArgumentDescriptor.CreateNumericDouble("Numeric Double Argument", "Sample Numeric Double Argument"),
                    ExpressionVariableArgumentDescriptor.CreateToggle("Toggle Argument", "Sample Toggle Argument"),
                    ExpressionVariableArgumentDescriptor.CreateOptions("Options Argument", "Sample Options Argument", "A", "B", "C", "D"),
                    ExpressionVariableArgumentDescriptor.CreateOptions<Options_e>("Enum Options Argument", "Sample Enum Options Argument"),
                    ExpressionVariableArgumentDescriptor.CreateMultipleOptions<MultiOptions_e>("Enum Multi Options Argument", "Sample Enum Multi Argument"),
                    new ExpressionVariableArgumentDescriptor("Default Argument", "Sample Default Argument")
                };
            }
            else if (string.Equals(variable.Name, "var2", StringComparison.CurrentCultureIgnoreCase))
            {
                dynamic = true;

                return new ExpressionVariableArgumentDescriptor[]
                {
                    ExpressionVariableArgumentDescriptor.CreateText("Argument1", "First Argument"),
                    ExpressionVariableArgumentDescriptor.CreateText("Argument2", "Second Argument")
                };
            }
            else if (string.Equals(variable.Name, "var3", StringComparison.CurrentCultureIgnoreCase)) 
            {
                dynamic = false;
                return null;
            }
            else 
            {
                dynamic = true;
                return null;
            }
        }

        public Brush GetBackground(IExpressionTokenVariable variable) => Brushes.Yellow;

        public BitmapImage GetIcon(IExpressionTokenVariable variable) 
        {
            if (string.Equals(variable.Name, "var1", StringComparison.CurrentCultureIgnoreCase))
            {
                return m_Var1Icon;
            }
            else 
            {
                return null;
            }
        }

        public string GetTitle(IExpressionTokenVariable variable) => $"_{variable.Name}_[{variable.Arguments?.Length} args(s)]";

        public string GetDescription(IExpressionTokenVariable variable) => $"Variable {variable.Name} with {variable.Arguments?.Length} argument(s)";
    }

    public class ExpressionBoxVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Expression1 { get; set; } = @"Some Text {var1} Some Other Text { var2 [par1] [par2] } \{a\}";
        public string Expression2 { get; set; } = "x{abc";

        public ICommand InsertVariableCommand { get; }

        private readonly BitmapImage m_Var1Icon;

        public ExpressionVariableLink[] VariableLinks { get; } 

        public ExpressionBoxVM() 
        {
            InsertVariableCommand = new RelayCommand<ExpressionBox>(InsertVariable);
            m_Var1Icon = Resources.icon.ToBitmapImage();
            m_Var1Icon.Freeze();

            VariableLinks = new ExpressionVariableLink[]
            {
                ExpressionVariableLink.Custom,
                new ExpressionVariableLink("Insert 'var1'...", "Inserting 'var1'", m_Var1Icon, s => new ExpressionTokenVariable("var1", null), true),
                new ExpressionVariableLink("Insert 'var2'", "Inserting 'var2'", null, s => new ExpressionTokenVariable("var2", new IExpressionToken[] { new ExpressionTokenText("X"), new ExpressionTokenText("Y") }), false)
            };
        }

        private void InsertVariable(ExpressionBox expBox)
        {
            expBox.Insert(new ExpressionTokenVariable("var3", null), false);
        }
    }
}
