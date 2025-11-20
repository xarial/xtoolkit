using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTests;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using Xarial.XToolkit.Services.Data.Attributes;
using NUnit.Framework;
using Newtonsoft.Json;
using Xarial.XToolkit.Services.Data;
using System.Drawing.Printing;

namespace Utils.Tests
{
    public class DataSerializerTests
    {
        #region Mocks

        public class SettsMock1
        {
            public string Field1 { get; set; }
            public double Field2 { get; set; }
        }

        [DataVersion("2.1.0", typeof(SettsMock2Transformer))]
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
                    o =>
                    {
                        var field2 = o.Children<JProperty>().First(p => p.Name == "Field2");

                        field2.Replace(new JProperty("Field3", field2.Value));
                        return o;
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
                    o =>
                    {
                        var field1 = o.Children<JProperty>().First(p => p.Name == "Field1");
                        field1.Value = new JValue(NewValue);

                        return o;
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

        [DataVersion("2.0", typeof(SettsMock5Transformer))]
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

        public enum Enum1_e
        {
            Val1,
            Val2
        }

        [DataSerializerOptions(EnumSerializationType_e.Text, DataFormatting_e.Indented, NullValueHandling_e.Include)]
        public class SettsMock7
        {
            public string Field1 { get; set; }
            public SettsChild1_1 Field2 { get; set; }
            public Enum1_e Field3 { get; set; }
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

        public class SettsMock8
        {
            public SettsChild2[] Children { get; set; }
        }

        public class SettsChild2 
        {
            public string Val { get; set; }

            [KnownKind(typeof(SettsSubChild1_1), "sc1")]
            [KnownKind(typeof(SettsSubChild1_2), "sc2")]
            public ISettsSubChild1 SubChild { get; set; }
        }

        public interface ISettsSubChild1 
        {
        }

        public class SettsSubChild1_1 : ISettsSubChild1
        {
        }

        public class SettsSubChild1_2 : ISettsSubChild1
        {
        }


        public class SettsMock9Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public SettsMock9Transformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version("1.0.0"), new Version("2.0.0"),
                    o =>
                    {
                        var prp1 = o.Property("_Field1");
                        prp1.Replace(new JProperty("Field1", prp1.Value));

                        var prp2 = o.Property("_Field2");
                        prp2.Replace(new JProperty("Field2", prp2.Value));

                        var prp3 = o.Property("_Field2_1");
                        prp3.Replace(new JProperty("Field2_1", prp3.Value));

                        var prp4 = o.Property("_Field3");
                        prp4.Replace(new JProperty("Field3", prp4.Value));

                        return o;
                    })
                };
            }
        }

        public class Field2Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public Field2Transformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version(), new Version("2.1.0"),
                    o =>
                    {
                        var prp1 = o.Property("__Value");
                        prp1.Replace(new JProperty("Value", prp1.Value));

                        return o;
                    })
                };
            }
        }

        public class Field2_1MockTransformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public Field2_1MockTransformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version("1.0.0"), new Version("2.2.0"),
                    o =>
                    {
                        var prp1 = o.Property("__Value");
                        prp1.Replace(new JProperty("Value", prp1.Value));

                        var prp2 = o.Property("_Value1");
                        prp2.Replace(new JProperty("Value1", prp2.Value));

                        return o;
                    })
                };
            }
        }

        public class Field3Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public Field3Transformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version("1.1.0"), new Version("2.3.0"),
                    o =>
                    {
                        var prp1 = o.Property("_Value");
                        prp1.Replace(new JProperty("Value", prp1.Value));

                        var prp2 = o.Property("_Child");
                        
                        if(prp2 != null)
                        {
                            prp2.Replace(new JProperty("Child", prp2.Value));
                        }

                        return o;
                    })
                };
            }
        }

        public class Field4Transformer : IVersionsTransformer
        {
            public IReadOnlyList<VersionTransform> Transforms { get; }

            public Field4Transformer()
            {
                Transforms = new VersionTransform[]
                {
                    new VersionTransform(new Version("1.0.0"), new Version("2.4.0"),
                    o =>
                    {
                        var prp1 = o.Property("_Value");
                        prp1.Replace(new JProperty("Value", prp1.Value));

                        return o;
                    })
                };
            }
        }

        [KnownKind(typeof(Field2Mock), "f2")]
        [KnownKind(typeof(Field2_1Mock), "f2_1")]
        [DataVersion("2.0.0", typeof(SettsMock9Transformer))]
        public class SettsMock9
        {
            public string Field1 { get; set; }
            public IField2Mock Field2 { get; set; }
            public IField2Mock Field2_1 { get; set; }
            public Field2Mock Field2_1_1 { get; set; }
            public Field3 Field3 { get; set; }

            [KnownKind(typeof(Field4_1), "f4_1")]
            public Field4[] Field4 { get; set; }
        }

        [DataVersion("2.1.0", typeof(Field2Transformer))]
        public interface IField2Mock
        {
            string Value { get; set; }
        }

        public class Field2Mock : IField2Mock
        {
            public string Value { get; set; }
        }

        [DataVersion("2.2.0", typeof(Field2_1MockTransformer))]
        public class Field2_1Mock : IField2Mock
        {
            public string Value { get; set; }
            public string Value1 { get; set; }
        }

        [DataVersion("2.3.0", typeof(Field3Transformer))]
        public class Field3
        {
            public string Value { get; set; }

            public Field3 Child { get; set; }
        }

        [DataVersion("2.4.0", typeof(Field4Transformer))]
        public class Field4 
        {
            public string Value { get; set; }
        }

        public class Field4_1 : Field4
        {
            public string Value1 { get; set; }
        }

        #endregion

        [Test]
        public void ReadTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock1>();
            var srv2 = new NsJsonDataSerializer<SettsMock2>();

            var mock1 = "{\"Field1\":\"AAA\",\"Field2\":10.0,\"$version\":\"0.0\"}";
            var mock2 = "{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}";

            var setts1 = srv1.Read(new StringReader(mock1));
            var setts2 = srv1.Read(new StringReader(mock2));
            var setts3 = srv2.Read(new StringReader(mock2));
            var setts4 = srv1.Read(new StringReader(mock1));
            var setts5 = srv1.Read(new StringReader(mock2));
            var setts6 = srv2.Read(new StringReader(mock1));
            var setts7 = srv2.Read(new StringReader(mock2));

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
        public void LegacyReadTest()
        {
            var srv = new NsJsonDataSerializer<SettsMock1>();

            var mock = "{\"Field1\":\"XYZ\",\"__version\":\"1.1.0\"}";

            var setts = srv.Read(new StringReader(mock));

            Assert.AreEqual("XYZ", setts.Field1);
            Assert.AreEqual(default(double), setts.Field2);
        }

        [Test]
        public void WriteTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock1>();
            var srv2 = new NsJsonDataSerializer<SettsMock2>();

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

            srv1.Save(setts1, new StringWriter(res1));
            srv2.Save(setts2, new StringWriter(res2));

            Assert.AreEqual("{\"Field1\":\"AAA\",\"Field2\":10.0}", res1.ToString());
            Assert.AreEqual("{\"Field1\":\"BBB\",\"Field3\":12.5,\"Field4\":true,\"$version\":\"2.1.0\"}", res2.ToString());
        }

        [Test]
        public void OptionsTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock7>();

            var setts1 = new SettsMock7()
            {
                Field1 = "A",
                Field2 = new SettsChild1_1()
                {
                    Par1 = "B",
                    Par2 = "C"
                },
                Field3 = Enum1_e.Val2
            };

            var res1 = new StringBuilder();

            srv1.Save(setts1, new StringWriter(res1));

            Assert.AreEqual("{\r\n  \"Field1\": \"A\",\r\n  \"Field2\": {\r\n    \"Par1\": \"B\",\r\n    \"Par2\": \"C\"\r\n  },\r\n  \"Field3\": \"Val2\"\r\n}", res1.ToString());
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

            var srv = new NsJsonDataSerializer<SettsMock3>(ser);

            srv.Save(setts, new StringWriter(res1));
            var res2 = srv.Read(new StringReader("{\"Obj\":\"ABC\"}"));

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

            var srv = new NsJsonDataSerializer<SettsMock4>(ser);

            srv.Save(setts, new StringWriter(res1));
            var res2 = srv.Read(new StringReader("{\"Obj\":\"ABC\"}"));

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

            var srv = new NsJsonDataSerializer<SettsMock2>(ser);

            srv.Save(setts, new StringWriter(res1));
            var res2 = srv.Read(new StringReader("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}"));

            Assert.AreEqual("{\"Field1\":\"ABC\",\"Field3\":0.0,\"Field4\":false,\"$version\":\"2.1.0\"}", res1.ToString());
            Assert.AreEqual("XYZ", res2.Field1);
        }

        [Test]
        public void VersionTest()
        {
            var setts = new SettsMock9()
            {
                Field1 = "A",
                Field2 = new Field2Mock() 
                {
                    Value = "B"
                },
                Field2_1 = new Field2_1Mock()
                {
                    Value = "C",
                    Value1 = "C1"
                },
                Field2_1_1 = new Field2Mock() 
                {
                    Value = "C1_1"
                },
                Field3 = new Field3() 
                {
                    Value = "D",
                    Child = new Field3() 
                    {
                        Value = "E",
                        Child = new Field3() 
                        {
                            Value = "F"
                        }
                    }
                },
                Field4 = new Field4[] 
                {
                    new Field4()
                    {
                        Value = "G",
                    },
                    new Field4()
                    {
                        Value = "H"
                    },
                    new Field4_1()
                    {
                        Value = "I",
                        Value1 = "I1"
                    }
                }
            };

            var res1 = new StringBuilder();

            var srv = new NsJsonDataSerializer<SettsMock9>(DataSerializerExtension.GetKnownKinds<SettsMock9>());

            var d1 = "{\"_Field1\":\"A\",\"_Field2\":{\"__Value\":\"B\",\"$kind\":\"f2\"},\"_Field2_1\":{\"__Value\":\"C\",\"_Value1\":\"C1\",\"$version\":\"1.0.0\",\"$kind\":\"f2_1\"},\"Field2_1_1\":{\"__Value\":\"C1_1\",\"$kind\":\"f2\"},\"_Field3\":{\"_Value\":\"D\",\"_Child\":{\"_Value\":\"E\",\"_Child\":{\"_Value\":\"F\",\"$version\":\"1.1.0\"},\"$version\":\"1.1.0\"},\"$version\":\"1.1.0\"},\"Field4\":[{\"_Value\":\"G\",\"$version\":\"1.0.0\"},{\"_Value\":\"H\",\"$version\":\"1.0.0\"},{\"Value1\":\"I1\",\"_Value\":\"I\",\"$version\":\"1.0.0\",\"$kind\":\"f4_1\"}],\"$version\":\"1.0.0\"}";

            srv.Save(setts, new StringWriter(res1));
            var res2 = srv.Read(new StringReader(d1));

            Assert.AreEqual("{\"Field1\":\"A\",\"Field2\":{\"Value\":\"B\",\"$version\":\"2.1.0\",\"$kind\":\"f2\"},\"Field2_1\":{\"Value\":\"C\",\"Value1\":\"C1\",\"$version\":\"2.2.0\",\"$kind\":\"f2_1\"},\"Field2_1_1\":{\"Value\":\"C1_1\",\"$version\":\"2.1.0\",\"$kind\":\"f2\"},\"Field3\":{\"Value\":\"D\",\"Child\":{\"Value\":\"E\",\"Child\":{\"Value\":\"F\",\"Child\":null,\"$version\":\"2.3.0\"},\"$version\":\"2.3.0\"},\"$version\":\"2.3.0\"},\"Field4\":[{\"Value\":\"G\",\"$version\":\"2.4.0\"},{\"Value\":\"H\",\"$version\":\"2.4.0\"},{\"Value1\":\"I1\",\"Value\":\"I\",\"$version\":\"2.4.0\",\"$kind\":\"f4_1\"}],\"$version\":\"2.0.0\"}", res1.ToString());

            Assert.AreEqual("A", res2.Field1);
            Assert.IsNotNull(res2.Field2);
            Assert.IsInstanceOf<Field2Mock>(res2.Field2);
            Assert.AreEqual("B", res2.Field2.Value);
            Assert.IsNotNull(res2.Field2_1);
            Assert.IsInstanceOf<Field2_1Mock>(res2.Field2_1);
            Assert.AreEqual("C", res2.Field2_1.Value);
            Assert.AreEqual("C1", ((Field2_1Mock)res2.Field2_1).Value1);
            Assert.IsNotNull(res2.Field3);
            Assert.AreEqual("D", res2.Field3.Value);
            Assert.IsNotNull(res2.Field3.Child);
            Assert.AreEqual("E", res2.Field3.Child.Value);
            Assert.IsNotNull(res2.Field3.Child.Child);
            Assert.AreEqual("F", res2.Field3.Child.Child.Value);
            Assert.IsNull(res2.Field3.Child.Child.Child);
            Assert.IsNotNull(res2.Field4);
            Assert.AreEqual(3, res2.Field4.Length);
            Assert.IsInstanceOf<Field4>(res2.Field4[0]);
            Assert.AreEqual("G", res2.Field4[0].Value);
            Assert.IsInstanceOf<Field4>(res2.Field4[1]);
            Assert.AreEqual("H", res2.Field4[1].Value);
            Assert.IsInstanceOf<Field4_1>(res2.Field4[2]);
            Assert.AreEqual("I", res2.Field4[2].Value);
            Assert.AreEqual("I1", ((Field4_1)res2.Field4[2]).Value1);
        }

        private class UserSettingsServiceTransformHandler : NsJsonDataSerializer<SettsMock5>
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

            var setts1 = srv.Read(new StringReader(mock1));

            Assert.AreEqual("BBB", setts1.Field1);
        }

        [Test]
        public void KnowTypesWriteTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock6>(DataSerializerExtension.GetKnownKinds<SettsMock6>());

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

            srv1.Save(setts1, new StringWriter(res1));

            Assert.AreEqual("{\"Children\":[{\"Par1\":\"A\",\"Par2\":\"B\",\"$kind\":\"sc1\"},{\"Par1\":\"A1\",\"Par3\":\"B1\",\"$kind\":\"sc2\"},{\"Par1\":\"A2\",\"Par2\":\"B2\",\"$kind\":\"sc1\"}]}", res1.ToString());
        }

        [Test]
        public void KnowTypesReadTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock6>(DataSerializerExtension.GetKnownKinds<SettsMock6>());

            var mock1 = "{\"Children\":[{\"Par1\":\"A\",\"Par2\":\"B\",\"$kind\":\"sc1\"},{\"Par1\":\"A1\",\"Par3\":\"B1\",\"$kind\":\"sc2\"},{\"Par1\":\"A2\",\"Par2\":\"B2\",\"$kind\":\"sc1\"}]}";

            var setts1 = srv1.Read(new StringReader(mock1));

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

        [Test]
        public void KnowTypesNullTest()
        {
            var srv1 = new NsJsonDataSerializer<SettsMock6>(DataSerializerExtension.GetKnownKinds<SettsMock6>());

            var mock1 = "{}";

            var setts1 = srv1.Read(new StringReader(mock1));

            var setts2 = new SettsMock6();

            var res1 = new StringBuilder();

            srv1.Save(setts2, new StringWriter(res1));
            
            var srv2 = new NsJsonDataSerializer<SettsMock8>(DataSerializerExtension.GetKnownKinds<SettsMock8>());

            var setts3 = new SettsMock8()
            {
                Children = new SettsChild2[] 
                {
                    new SettsChild2()
                    {
                        Val = "1"
                    }
                }
            };

            var res2 = new StringBuilder();

            srv2.Save(setts3, new StringWriter(res2));

            var mock2 = "{\"Children\":[{\"Val\":\"1\"}]}";

            var setts4 = srv2.Read(new StringReader(mock2));

            Assert.IsNotNull(setts1);
            Assert.IsNull(setts1.Children);
            Assert.AreEqual("{\"Children\":null}", res1.ToString());
            Assert.AreEqual("{\"Children\":[{\"Val\":\"1\",\"SubChild\":null}]}", res2.ToString());
            Assert.IsNotNull(setts4);
            Assert.AreEqual(1, setts4.Children.Length);
            Assert.AreEqual("1", setts4.Children[0].Val);
            Assert.IsNull(setts4.Children[0].SubChild);
        }
    }
}