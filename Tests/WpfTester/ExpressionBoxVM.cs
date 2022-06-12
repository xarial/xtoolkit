using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Controls;

namespace WpfTester
{
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
