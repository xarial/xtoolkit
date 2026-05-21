using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib
{
    public interface ISampleClass 
    {
        void Foo(string param);
    }

    public class SampleClass : ISampleClass
    {
        public void Foo(string param)
        {
            Console.WriteLine($"Foo 1.0.0.0: '{param}'. Domain: {AppDomain.CurrentDomain.FriendlyName}");
        }
    }
}
