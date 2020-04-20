//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.UserSettings.Attributes;
using Xarial.XToolkit.Services.UserSettings.Converters;

namespace Xarial.XToolkit.Services.UserSettings
{
    public class UserSettingsService
    {
        public T ReadSettings<T>(TextReader settsReader)
        {
            var jsonSer = new JsonSerializer();

            if (TryGetVersionInfo<T>(out Version vers, out IEnumerable<VersionTransform> transform))
            {
                jsonSer.Converters.Add(new ReadSettingsJsonConverter(typeof(T), transform, vers));
            }

            return (T)jsonSer.Deserialize(settsReader, typeof(T));
        }

        public void StoreSettings<T>(T setts, TextWriter settsWriter)
        {
            var jsonSer = new JsonSerializer();

            if (TryGetVersionInfo<T>(out Version vers, out _))
            {
                jsonSer.Converters.Add(new WriteSettingsJsonConverter(typeof(T), vers));
            }
            
            jsonSer.Serialize(settsWriter, setts, typeof(T));
        }

        private bool TryGetVersionInfo<T>(out Version vers, out IEnumerable<VersionTransform> transforms)
        {
            if (typeof(T).TryGetAttribute(out UserSettingVersionAttribute att, true))
            {
                vers = att.Version;
                transforms = att.VersionTransformers;
                return true;
            }
            else
            {
                vers = null;
                transforms = null;
                return false;
            }
        }
    }

    public static class UserSettingsServiceExtension
    {
        public static T ReadSettings<T>(this UserSettingsService settsSvc, string settsFile)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.ReadSettings<T>(textReader);
            }
        }

        public static void StoreSettings<T>(this UserSettingsService settsSvc, T setts, string settsFile)
        {
            var settsDir = Path.GetDirectoryName(settsFile);

            if (!Directory.Exists(settsDir))
            {
                Directory.CreateDirectory(settsDir);
            }

            using (var textWriter = File.CreateText(settsFile))
            {
                settsSvc.StoreSettings<T>(setts, textWriter);
            }
        }
    }
}
