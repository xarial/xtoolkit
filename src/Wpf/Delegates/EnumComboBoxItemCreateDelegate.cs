using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Wpf.Delegates
{
    public class EnumComboBoxItemArgument 
    {
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }

    public delegate void EnumComboBoxItemCreateDelegate(Enum item, EnumComboBoxItemArgument arg);
}
