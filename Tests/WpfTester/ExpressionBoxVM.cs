using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Controls;

namespace WpfTester
{
    public class MyExpressionVariableDescriptor : IExpressionVariableDescriptor
    {
        public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic)
        {
            if (string.Equals(variable.Name, "var1", StringComparison.CurrentCultureIgnoreCase))
            {
                dynamic = false;

                return new ExpressionVariableArgumentDescriptor[]
                {
                    ExpressionVariableArgumentDescriptor.CreateText("Text Argument", "Sample Text Argument")
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
            else 
            {
                dynamic = true;
                return null;
            }
        }

        public Brush GetBackground(IExpressionTokenVariable variable) => Brushes.Yellow;

        public BitmapImage GetIcon(IExpressionTokenVariable variable) => null;

        public string GetTitle(IExpressionTokenVariable variable) => $"_{variable.Name}_[{variable.Arguments?.Length} args(s)]";

        public string GetTooltip(IExpressionTokenVariable variable) => null;
    }

    public class ExpressionBoxVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Expression1 { get; set; } = @"Some Text {var1} Some Other Text { var2 [par1] [par2] } \{a\}";
        public string Expression2 { get; set; } = "x{abc";

        public ICommand InsertVariableCommand { get; }

        public ExpressionBoxVM() 
        {
            InsertVariableCommand = new RelayCommand<ExpressionBox>(InsertVariable);
        }

        private void InsertVariable(ExpressionBox expBox)
        {
            expBox.Insert(new ExpressionTokenVariable("var3", null));
        }
    }
}
