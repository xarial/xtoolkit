//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xarial.XToolkit.Wpf.Extensions
{
    public static class NotifyPropertyChangedExtension
    {
        public static void NotifyChanged(this INotifyPropertyChanged prpChanged, [CallerMemberName] string prpName = "")
        {
            var curType = prpChanged.GetType();
            FieldInfo eventField = null;

            while (eventField == null) 
            {
                eventField = curType.GetField(nameof(INotifyPropertyChanged.PropertyChanged),
                    BindingFlags.Instance | BindingFlags.NonPublic);

                curType = curType.BaseType;
            }

            var eventDelegate = (PropertyChangedEventHandler)eventField.GetValue(prpChanged);

            if (eventDelegate != null)
            {
                var eventArgs = new PropertyChangedEventArgs(prpName);

                foreach (PropertyChangedEventHandler handler in eventDelegate.GetInvocationList())
                {
                    handler.Invoke(prpChanged, eventArgs);
                }
            }
        }
    }
}
