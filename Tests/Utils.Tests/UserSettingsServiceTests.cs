using Xarial.XToolkit.Services.UserSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTests;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using Xarial.XToolkit.Services.UserSettings.Attributes;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Xarial.XToolkit.Services.UserSettings.Tests
{
    public class UserSettingsServiceTests
    {
        #region Mocks

        public class SettsMock1
        {
            public string Field1 { get; set; }
            public double Field2 { get; set; }
        }

        [UserSettingVersion("2.1.0", typeof(SettsMock2Transformer))]
        public class SettsMock2
        {
            public string Field1 { get; set; }
            public double Field3 { get; set; }
            public bool Field4 { get; set; }
        }

        public class SettsMock2Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms => m_Transforms;

            private List<VersionTransform> m_Transforms;

            public SettsMock2Transformer()
            {
                m_Transforms = new List<VersionTransform>();
                m_Transforms.Add(new VersionTransform(
                    new Version(),
                    new Version("2.1.0"),
                    t =>
                    {
                        var field2 = t.Children<JProperty>().First(p => p.Name == "Field2");

                        field2.Replace(new JProperty("Field3", (field2 as JProperty).Value));
                        return t;
                    }));
            }
        }

        public class SettsMock5Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms => m_Transforms;

            private List<VersionTransform> m_Transforms;

            public string NewValue { get; set; }

            public SettsMock5Transformer()
            {
                m_Transforms = new List<VersionTransform>();
                m_Transforms.Add(new VersionTransform(
                    new Version("1.0"),
                    new Version("2.0"),
                    t =>
                    {
                        var field1 = t.Children<JProperty>().First(p => p.Name == "Field1");
                        field1.Value = new JValue(NewValue);

                        return t;
                    }));
            }
        }

        public class SettsMock3 
        {
            public ObjectType Obj { get; set; }
        }

        public class SettsMock4
        {
            public IObjectType Obj { get; set; }
        }

        [UserSettingVersion("2.0", typeof(SettsMock5Transformer))]
        public class SettsMock5 
        {
            public string Field1 { get; set; }
        }

        public interface IObjectType 
        {
            string Value { get; set; }
        }

        public class ObjectType
        {
            public string Value { get; set; }
        }

        public class ObjectType2 : IObjectType
        {
            public string Value { get; set; }
        }

        #endregion

        [Test]
        public void ReadSettingsTest()
        {
            var srv = new UserSettingsService();

            var mock1 = "{\"Field1\":\"AAA\",\"Field2\":10.0,\"$version\":\"0.0\"}";
            var mock2 = "{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}";

            var setts1 = srv.ReadSettings<SettsMock1>(new StringReader(mock1));
            var setts2 = srv.ReadSettings<SettsMock1>(new StringReader(mock2));
            var setts3 = srv.ReadSettings<SettsMock2>(new StringReader(mock2));
            var setts4 = srv.ReadSettings<SettsMock1>(new StringReader(mock1));
            var setts5 = srv.ReadSettings<SettsMock1>(new StringReader(mock2));
            var setts6 = srv.ReadSettings<SettsMock2>(new StringReader(mock1));
            var setts7 = srv.ReadSettings<SettsMock2>(new StringReader(mock2));

            Assert.AreEqual("AAA", setts1.Field1);
            Assert.AreEqual(10, setts1.Field2);
            Assert.AreEqual("BBB", setts2.Field1);
            Assert.AreEqual(default(double), setts2.Field2);
            Assert.AreEqual("BBB", setts3.Field1);
            Assert.AreEqual(12.5, setts3.Field3);
            Assert.AreEqual(true, setts3.Field4);

            Assert.AreEqual("AAA", setts4.Field1);
            Assert.AreEqual(10, setts4.Field2);
            Assert.AreEqual("BBB", setts5.Field1);
            Assert.AreEqual(default(double), setts5.Field2);
            Assert.AreEqual("AAA", setts6.Field1);
            Assert.AreEqual(10, setts6.Field3);
            Assert.AreEqual(default(bool), setts6.Field4);
            Assert.AreEqual("BBB", setts7.Field1);
            Assert.AreEqual(12.5, setts7.Field3);
            Assert.AreEqual(true, setts7.Field4);
        }

        [Test]
        public void LegacyReadSettingsTest()
        {
            var srv = new UserSettingsService();

            var mock = "{\"Field1\":\"XYZ\",\"__version\":\"1.1.0\"}";

            var setts = srv.ReadSettings<SettsMock1>(new StringReader(mock));

            Assert.AreEqual("XYZ", setts.Field1);
            Assert.AreEqual(default(double), setts.Field2);
        }

        [Test]
        public void WriteSettingsTest()
        {
            var srv = new UserSettingsService();

            var setts1 = new SettsMock1()
            {
                Field1 = "AAA",
                Field2 = 10
            };

            var setts2 = new SettsMock2()
            {
                Field1 = "BBB",
                Field3 = 12.5,
                Field4 = true
            };

            var res1 = new StringBuilder();
            var res2 = new StringBuilder();

            srv.StoreSettings(setts1, new StringWriter(res1));
            srv.StoreSettings(setts2, new StringWriter(res2));

            Assert.AreEqual("{\"Field1\":\"AAA\",\"Field2\":10.0}", res1.ToString());
            Assert.AreEqual("{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}", res2.ToString());
        }

        [Test]
        public void CustomConverterTest() 
        {
            var srv = new UserSettingsService();

            var setts = new SettsMock3()
            {
                Obj = new ObjectType()
                {
                    Value = "XYZ"
                }
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<ObjectType>(x => x.Value, x => new ObjectType() { Value = x });

            srv.StoreSettings(setts, new StringWriter(res1), ser);
            var res2 = srv.ReadSettings<SettsMock3>(new StringReader("{\"Obj\":\"ABC\"}"), ser);

            Assert.AreEqual("{\"Obj\":\"XYZ\"}", res1.ToString());
            Assert.AreEqual("ABC", res2.Obj.Value);
        }

        [Test]
        public void CustomConverterDerivedTypeTest()
        {
            var srv = new UserSettingsService();

            var setts = new SettsMock4()
            {
                Obj = new ObjectType2()
                {
                    Value = "XYZ"
                }
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<IObjectType>(x => x.Value, x => new ObjectType2() { Value = x });

            srv.StoreSettings(setts, new StringWriter(res1), ser);
            var res2 = srv.ReadSettings<SettsMock4>(new StringReader("{\"Obj\":\"ABC\"}"), ser);

            Assert.AreEqual("{\"Obj\":\"XYZ\"}", res1.ToString());
            Assert.AreEqual("ABC", res2.Obj.Value);
        }

        [Test]
        public void CustomConverterAndVersionTest()
        {
            var srv = new UserSettingsService();

            var setts = new SettsMock2()
            {
                Field1 = ""
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<string>(x => "ABC", x => "XYZ");

            srv.StoreSettings(setts, new StringWriter(res1), ser);
            var res2 = srv.ReadSettings<SettsMock2>(new StringReader("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}"), ser);

            Assert.AreEqual("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}", res1.ToString());
            Assert.AreEqual("XYZ", res2.Field1);
        }

        [Test]
        public void TransformHandlerTest() 
        {
            var srv = new UserSettingsService();

            var mock1 = "{\"Field1\":\"AAA\",\"$version\":\"1.0\"}";

            var setts1 = srv.ReadSettings<SettsMock5>(new StringReader(mock1), t => { ((SettsMock5Transformer)t).NewValue = "BBB"; return t; });
            
            Assert.AreEqual("BBB", setts1.Field1);
        }
    }
}