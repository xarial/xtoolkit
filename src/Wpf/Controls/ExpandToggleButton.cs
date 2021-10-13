using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class ExpandToggleButton : ToggleButton
    {
        static ExpandToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpandToggleButton),
                new FrameworkPropertyMetadata(typeof(ExpandToggleButton)));
        }
    }
}
