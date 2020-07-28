using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Extensions;

namespace Wpf.Tests
{
    public class NotifyPropertyChangedExtensionTest
    {
        public class MockVM : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string m_TestPrp;

            public string TestPrp
            {
                get => m_TestPrp;
                set 
                {
                    m_TestPrp = value;
                    this.NotifyChanged();
                }
            }
        }
        
        public class Mock1VM : MockVM
        {
        }

        [Test]
        public void CurrentClassTest()
        {
            var vm = new MockVM();
            string changedPrp = "";

            vm.PropertyChanged += (s, a) => { changedPrp += a.PropertyName; };

            vm.TestPrp = "A";

            Assert.AreEqual("TestPrp", changedPrp);
        }

        [Test]
        public void InheritedClassTest()
        {
            var vm = new Mock1VM();
            string changedPrp = "";

            vm.PropertyChanged += (s, a) => { changedPrp += a.PropertyName; };

            vm.TestPrp = "A";

            Assert.AreEqual("TestPrp", changedPrp);
        }

        [Test]
        public void MultipleHandlersTest()
        {
            var vm = new MockVM();
            string changedPrp = "";

            vm.PropertyChanged += (s, a) => { changedPrp += a.PropertyName; };
            vm.PropertyChanged += (s, a) => { changedPrp += a.PropertyName; };
            vm.PropertyChanged += (s, a) => { changedPrp += a.PropertyName; };

            vm.TestPrp = "A";

            Assert.AreEqual("TestPrpTestPrpTestPrp", changedPrp);
        }
    }
}
