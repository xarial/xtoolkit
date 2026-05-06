//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTester
{
    public class NumberBoxVM
    {
        public double DoubleVal { get; set; }
        public int IntVal { get; set; }

        public NumberBoxVM() 
        {
            DoubleVal = 10.0;
            IntVal = 2;
        }
    }
}
