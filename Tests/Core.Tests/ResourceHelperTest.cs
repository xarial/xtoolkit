using NUnit.Framework;
using System;
using Xarial.XToolkit.Reflection;

namespace CoreTests
{
    public class ResourceHelperTest
    {
        #region Mocks

        public static class ResourceMock1
        {
            public static string TestString { get; set; } = "Test";
        }

        public class SettingsMock
        {
            public static SettingsMock Default { get; } = new SettingsMock();

            public string TestUserSettings { get; } = "Test2";
            public string TestAppSettings { get; } = "Test1";
        }

        public static class ResourceMock
        {
            public class SubClass
            {
                public class SubClass2
                {
                    public string Val { get; set; } = "AAA";
                }

                public SubClass2 Cl2 { get; set; } = new SubClass2();
            }

            public static SubClass Cl1 { get; set; } = new SubClass();
        }

        #endregion

        [Test]
        public void TestGetResource()
        {
            var r1 = ResourceHelper.GetResource<string>(typeof(ResourceMock1), nameof(ResourceMock1.TestString));
            var r2 = ResourceHelper.GetResource<string>(typeof(SettingsMock), nameof(SettingsMock.Default) + "." + nameof(SettingsMock.TestUserSettings));
            var r3 = ResourceHelper.GetResource<string>(typeof(SettingsMock), nameof(SettingsMock.Default) + "." + nameof(SettingsMock.TestAppSettings));
            var r4 = ResourceHelper.GetResource<string>(typeof(ResourceMock), "Cl1.Cl2.Val");

            Assert.AreEqual("Test", r1);
            Assert.AreEqual("Test2", r2);
            Assert.AreEqual("Test1", r3);
            Assert.AreEqual("AAA", r4);
            Assert.Throws<NullReferenceException>(() => ResourceHelper.GetResource<string>(typeof(ResourceMock), "Cl2.Cl2.Val"));
        }
    }
}
