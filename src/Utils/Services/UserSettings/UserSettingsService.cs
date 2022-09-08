﻿//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.UserSettings.Attributes;
using Xarial.XToolkit.Services.UserSettings.Converters;

namespace Xarial.XToolkit.Services.UserSettings
{
    public class UserSettingsService
    {
        public T ReadSettings<T>(TextReader settsReader, params IValueSerializer[] serializers)
            => ReadSettings<T>(settsReader, null, serializers);

        public T ReadSettings<T>(TextReader settsReader, Func<IVersionsTransformer, IVersionsTransformer> versTransformerHandler, params IValueSerializer[] serializers)
        {
            var jsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo<T>(out Version vers, out IVersionsTransformer transform))
            {
                if (versTransformerHandler != null)
                {
                    transform = versTransformerHandler.Invoke(transform);
                }

                jsonSer.Converters.Add(new ReadSettingsJsonConverter(typeof(T), transform?.Transforms ?? Enumerable.Empty<VersionTransform>(), vers));
            }

            foreach (var ser in serializers)
            {
                jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
            }

            return (T)jsonSer.Deserialize(settsReader, typeof(T));
        }

        public void StoreSettings<T>(T setts, TextWriter settsWriter, params IValueSerializer[] serializers)
        {
            var jsonSer = CreateJsonSerializer();

            if (TryGetVersionInfo<T>(out Version vers, out _))
            {
                jsonSer.Converters.Add(new WriteSettingsJsonConverter(typeof(T), vers));
            }

            foreach (var ser in serializers)
            {
                jsonSer.Converters.Add(new CustomSerializerJsonConverter(ser));
            }

            jsonSer.Serialize(settsWriter, setts, typeof(T));
        }

        protected virtual JsonSerializer CreateJsonSerializer() => new JsonSerializer();

        private bool TryGetVersionInfo<T>(out Version vers, out IVersionsTransformer transforms)
        {
            if (typeof(T).TryGetAttribute(out UserSettingVersionAttribute att, true))
            {
                vers = att.Version;
                transforms = att.VersionTransformer;
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
        public static T ReadSettings<T>(this UserSettingsService settsSvc, string settsFile, params IValueSerializer[] serializers)
        {
            using (var textReader = File.OpenText(settsFile))
            {
                return settsSvc.ReadSettings<T>(textReader, serializers);
            }
        }

        public static void StoreSettings<T>(this UserSettingsService settsSvc, T setts, string settsFile, params IValueSerializer[] serializers)
        {
            var settsDir = Path.GetDirectoryName(settsFile);

            if (!Directory.Exists(settsDir))
            {
                Directory.CreateDirectory(settsDir);
            }

            using (var textWriter = File.CreateText(settsFile))
            {
                settsSvc.StoreSettings<T>(setts, textWriter, serializers);
            }
        }
    }
}
