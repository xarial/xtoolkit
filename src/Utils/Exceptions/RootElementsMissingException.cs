using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Exceptions
{
    /// <summary>
    /// Exception from <see cref="Helpers.Hierarchy.Order{T}(IEnumerable{T}, Func{T, IEnumerable{T}}, IEqualityComparer{T})"/>
    /// </summary>
    public class RootElementsMissingException : Exception
    {
        /// <summary>
        /// Defautl constructor
        /// </summary>
        public RootElementsMissingException() : base("Failed to find root elements. This happens if there a circular dependency in the hierarchy") 
        {
        }
    }
}
