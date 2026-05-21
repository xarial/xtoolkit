//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Extensions;

namespace Lib.Wpf
{
    public class PasswordBoxExVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Password
        {
            get => m_Password;
            set 
            {
                m_Password = value;
                this.NotifyChanged();
            }
        }

        private string m_Password;

        public PasswordBoxExVM()
        {
            m_Password = "ABC";
        }
    }
}
