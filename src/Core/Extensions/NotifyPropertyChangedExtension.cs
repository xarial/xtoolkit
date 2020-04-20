using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xarial.XToolkit.Extensions
{
    public static class NotifyPropertyChangedExtension
    {
        public static void NotifyChanged(this INotifyPropertyChanged prpChanged, [CallerMemberName] string prpName = "")
        {
            var eventDelegate = (MulticastDelegate)prpChanged.GetType().GetField(
                nameof(INotifyPropertyChanged.PropertyChanged), 
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prpChanged);

            if (eventDelegate != null)
            {
                var eventArgs = new PropertyChangedEventArgs(prpName);

                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { prpChanged, eventArgs });
                }
            }
        }
    }
}
