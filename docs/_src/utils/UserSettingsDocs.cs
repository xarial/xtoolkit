using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Utils.Docs
{
    public static class UserSettingsDocs
    {
        public static class V1
        {
            //--- v1
            [UserSettingVersion("1.0.0", typeof(UserSettingsVersionTransformer))]
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
            [UserSettingVersion("2.0.0", typeof(UserSettingsVersionTransformer))]
            public class UserSettings
            {
                public string TextField { get; set; }
                public double Field2 { get; set; }
                public bool Field3 { get; set; }
            }
            //---
        }

        //--- v3
        [UserSettingVersion("3.0.0", typeof(UserSettingsVersionTransformer))]
        public class UserSettings
        {
            public string TextField { get; set; }
            public double DoubleField { get; set; }
            public bool BoolField { get; set; }
        }
        //---

        //--- transformer
        public class UserSettingsVersionTransformer : BaseUserSettingsVersionsTransformer
        {
            public UserSettingsVersionTransformer()
            {
                Add(new Version("1.0.0"), new Version("2.0.0"), t =>
                {
                    var field1 = t.Children<JProperty>().First(p => p.Name == "Field1");
                    field1.Replace(new JProperty("TextField", (field1 as JProperty).Value));
                    return t;
                });

                Add(new Version("2.0.0"), new Version("3.0.0"), t =>
                {
                    var field2 = t.Children<JProperty>().First(p => p.Name == "Field2");
                    field2.Replace(new JProperty("DoubleField", (field2 as JProperty).Value));

                    var field3 = t.Children<JProperty>().First(p => p.Name == "Field3");
                    field3.Replace(new JProperty("BoolField", (field3 as JProperty).Value));

                    return t;
                });
            }
        }
        //---

        public static void StoreSettings() 
        {
            //--- store
            var svc = new UserSettingsService();
            
            var userSetts = new UserSettings();

            svc.StoreSettings(userSetts,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "my-app-settings.json"));

            //---
        }

        public static void ReadSettings()
        {
            //--- read
            var svc = new UserSettingsService();

            var userSetts = svc.ReadSettings<UserSettings>(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "my-app-settings.json"));
            //---
        }
    }
}
