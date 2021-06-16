//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Xarial.XToolkit.Helpers
{
    internal static class JsonFileSerializer
    {
        internal static void SerializeToFile<T>(T obj, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(obj));
        }
    }
}
