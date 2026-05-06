//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Xarial.XToolkit.Services.Data
{
    /// <summary>
    /// Service to provide the transformation between versions in <see cref="IDataSerializer"/>
    /// </summary>
    public interface IVersionsTransformer 
    {
        /// <summary>
        /// List of transformations
        /// </summary>
        IReadOnlyList<VersionTransform> Transforms { get; }
    }
}
