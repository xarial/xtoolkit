using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Services.Data;
using Xarial.XToolkit.Services.Data.Attributes;

namespace Utils.Docs
{
    public static class UserSettingsDocs
    {
        public static class V1
        {
            //--- v1
            [DataVersion("1.0.0", typeof(UserSettingsVersionTransformer))]
            public class UserSettings
            {
                public string Field1 { get; set; }
                public double Field2 { get; set; }
                public bool Field3 { get; set; }
            }
            //---
        }

        public static class V2
        {
            //--- v2
            [DataVersion("2.0.0", typeof(UserSettingsVersionTransformer))]
            public class UserSettings
            {
                public string TextField { get; set; }
                public double Field2 { get; set; }
                public bool Field3 { get; set; }
            }
            //---
        }

        //--- v3
        [DataVersion("3.0.0", typeof(UserSettingsVersionTransformer))]
        public class UserSettings
        {
            public string TextField { get; set; }
            public double DoubleField { get; set; }
            public bool BoolField { get; set; }
        }
        //---

        //--- transformer
        public class UserSettingsVersionTransformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public UserSettingsVersionTransformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version("1.0.0"), new Version("2.0.0"), t =>
                    {
                        var field1 = ((JObject)t).Property("Field1");
                        field1.Replace(new JProperty("TextField", field1.Value));
                        return t;
                    }),
                    new VersionTransform(new Version("2.0.0"), new Version("3.0.0"), t =>
                    {
                        var field2 = ((JObject)t).Property("Field2");
                        field2.Replace(new JProperty("DoubleField", field2.Value));

                        var field3 = ((JObject)t).Property("Field3");
                        field3.Replace(new JProperty("BoolField", field3.Value));

                        return t;
                    })
                };
            }
        }
        //---

        public static void StoreSettings() 
        {
            //--- store
            var svc = new NsJsonDataSerializer<UserSettings>();
            
            var userSetts = new UserSettings();

            svc.Save(userSetts,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "my-app-settings.json"));

            //---
        }

        public static void ReadSettings()
        {
            //--- read
            var svc = new NsJsonDataSerializer<UserSettings>();

            var userSetts = svc.Read(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "my-app-settings.json"));
            //---
        }
    }
}
