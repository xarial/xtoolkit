//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Wpf.Controls
{
    internal static class EnumControlHelper
    {
        internal static string GetTitle(Enum value)
        {
            string title = "";

            if (value != null)
            {
                if (!value.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName))
                {
                    if (Convert.ToInt32(value) != 0 || Enum.IsDefined(value.GetType(), value))
                    {
                        title = value.ToString();
                    }
                }
            }

            return title;
        }

        internal static string GetDescription(Enum value)
        {
            string title = "";

            if (value != null)
            {
                if (!value.TryGetAttribute<DescriptionAttribute>(a => title = a.Description))
                {
                    title = GetTitle(value);

                    if (string.IsNullOrEmpty(title))
                    {
                        title = value.ToString();
                    }
                }
            }

            return title;
        }
    }
}
