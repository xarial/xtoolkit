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

        public class SettsMock6 
        {
            public ISettsChild1[] Children { get; set; }
        }

        [KnownKind(typeof(SettsChild1_1), "sc1")]
        [KnownKind(typeof(SettsChild1_2), "sc2")]
        public interface ISettsChild1 
        {
            string Par1 { get; }
        }

        public class SettsChild1_1 : ISettsChild1
        {
            public string Par1 { get; set; }
            public string Par2 { get; set; }
        }

        public class SettsChild1_2 : ISettsChild1
        {
            public string Par1 { get; set; }
            public string Par3 { get; set; }
        }

        #endregion

        [Test]
        public void ReadSettingsTest()
        {
            var srv1 = new UserSettingsService<SettsMock1>();
            var srv2 = new UserSettingsService<SettsMock2>();

            var mock1 = "{\"Field1\":\"AAA\",\"Field2\":10.0,\"$version\":\"0.0\"}";
            var mock2 = "{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}";

            var setts1 = srv1.ReadSettings(new StringReader(mock1));
            var setts2 = srv1.ReadSettings(new StringReader(mock2));
            var setts3 = srv2.ReadSettings(new StringReader(mock2));
            var setts4 = srv1.ReadSettings(new StringReader(mock1));
            var setts5 = srv1.ReadSettings(new StringReader(mock2));
            var setts6 = srv2.ReadSettings(new StringReader(mock1));
            var setts7 = srv2.ReadSettings(new StringReader(mock2));

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
            var srv = new UserSettingsService<SettsMock1>();

            var mock = "{\"Field1\":\"XYZ\",\"__version\":\"1.1.0\"}";

            var setts = srv.ReadSettings(new StringReader(mock));

            Assert.AreEqual("XYZ", setts.Field1);
            Assert.AreEqual(default(double), setts.Field2);
        }

        [Test]
        public void WriteSettingsTest()
        {
            var srv1 = new UserSettingsService<SettsMock1>();
            var srv2 = new UserSettingsService<SettsMock2>();

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

            srv1.StoreSettings(setts1, new StringWriter(res1));
            srv2.StoreSettings(setts2, new StringWriter(res2));

            Assert.AreEqual("{\"Field1\":\"AAA\",\"Field2\":10.0}", res1.ToString());
            Assert.AreEqual("{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}", res2.ToString());
        }

        [Test]
        public void CustomConverterTest() 
        {
            var setts = new SettsMock3()
            {
                Obj = new ObjectType()
                {
                    Value = "XYZ"
                }
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<ObjectType>(x => x.Value, x => new ObjectType() { Value = x });

            var srv = new UserSettingsService<SettsMock3>(ser);

            srv.StoreSettings(setts, new StringWriter(res1));
            var res2 = srv.ReadSettings(new StringReader("{\"Obj\":\"ABC\"}"));

            Assert.AreEqual("{\"Obj\":\"XYZ\"}", res1.ToString());
            Assert.AreEqual("ABC", res2.Obj.Value);
        }

        [Test]
        public void CustomConverterDerivedTypeTest()
        {
            var setts = new SettsMock4()
            {
                Obj = new ObjectType2()
                {
                    Value = "XYZ"
                }
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<IObjectType>(x => x.Value, x => new ObjectType2() { Value = x });

            var srv = new UserSettingsService<SettsMock4>(ser);

            srv.StoreSettings(setts, new StringWriter(res1));
            var res2 = srv.ReadSettings(new StringReader("{\"Obj\":\"ABC\"}"));

            Assert.AreEqual("{\"Obj\":\"XYZ\"}", res1.ToString());
            Assert.AreEqual("ABC", res2.Obj.Value);
        }

        [Test]
        public void CustomConverterAndVersionTest()
        {
            var setts = new SettsMock2()
            {
                Field1 = ""
            };

            var res1 = new StringBuilder();

            var ser = new BaseValueSerializer<string>(x => "ABC", x => "XYZ");

            var srv = new UserSettingsService<SettsMock2>(ser);

            srv.StoreSettings(setts, new StringWriter(res1));
            var res2 = srv.ReadSettings(new StringReader("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}"));

            Assert.AreEqual("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}", res1.ToString());
            Assert.AreEqual("XYZ", res2.Field1);
        }

        private class UserSettingsServiceTransformHandler : UserSettingsService<SettsMock5> 
        {
            protected override IVersionsTransformer GetVersionTransformer(IVersionsTransformer src)
            {
                ((SettsMock5Transformer)src).NewValue = "BBB";
                return src;
            }
        }

        [Test]
        public void TransformHandlerTest() 
        {
            var srv = new UserSettingsServiceTransformHandler();

            var mock1 = "{\"Field1\":\"AAA\",\"$version\":\"1.0\"}";

            var setts1 = srv.ReadSettings(new StringReader(mock1));
            
            Assert.AreEqual("BBB", setts1.Field1);
        }

        [Test]
        public void KnowTypesWriteTest() 
        {
            var srv1 = new UserSettingsService<SettsMock6>(UserSettingsService.GetKnownTypes(typeof(SettsMock6)));

            var setts1 = new SettsMock6()
            {
                Children = new ISettsChild1[] 
                {
                    new SettsChild1_1()
                    {
                        Par1 = "A",
                        Par2 = "B",
                    },
                    new SettsChild1_2()
                    {
                        Par1 = "A1",
                        Par3 = "B1",
                    },
                    new SettsChild1_1()
                    {
                        Par1 = "A2",
                        Par2 = "B2",
                    }
                }
            };

            var res1 = new StringBuilder();

            srv1.StoreSettings(setts1, new StringWriter(res1));

            Assert.AreEqual("{\"Children\":[{\"Par1\":\"A\",\"Par2\":\"B\",\"$kind\":\"sc1\"},{\"Par1\":\"A1\",\"Par3\":\"B1\",\"$kind\":\"sc2\"},{\"Par1\":\"A2\",\"Par2\":\"B2\",\"$kind\":\"sc1\"}]}", res1.ToString());
        }

        [Test]
        public void KnowTypesReadTest()
        {
            var srv1 = new UserSettingsService<SettsMock6>(UserSettingsService.GetKnownTypes(typeof(SettsMock6)));

            var mock1 = "{\"Children\":[{\"Par1\":\"A\",\"Par2\":\"B\",\"$kind\":\"sc1\"},{\"Par1\":\"A1\",\"Par3\":\"B1\",\"$kind\":\"sc2\"},{\"Par1\":\"A2\",\"Par2\":\"B2\",\"$kind\":\"sc1\"}]}";

            var setts1 = srv1.ReadSettings(new StringReader(mock1));

            Assert.AreEqual(3, setts1.Children.Length);
            Assert.IsInstanceOf<SettsChild1_1>(setts1.Children[0]);
            Assert.AreEqual("A", ((SettsChild1_1)setts1.Children[0]).Par1);
            Assert.AreEqual("B", ((SettsChild1_1)setts1.Children[0]).Par2);
            Assert.IsInstanceOf<SettsChild1_2>(setts1.Children[1]);
            Assert.AreEqual("A1", ((SettsChild1_2)setts1.Children[1]).Par1);
            Assert.AreEqual("B1", ((SettsChild1_2)setts1.Children[1]).Par3);
            Assert.IsInstanceOf<SettsChild1_1>(setts1.Children[2]);
            Assert.AreEqual("A2", ((SettsChild1_1)setts1.Children[2]).Par1);
            Assert.AreEqual("B2", ((SettsChild1_1)setts1.Children[2]).Par2);
        }
    }
}