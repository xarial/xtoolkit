//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Xarial.XToolkit.Services.UserSettings
{
    public interface IVersionsTransformer 
    {
        IReadOnlyList<VersionTransform> Transforms { get; }
    }
}
