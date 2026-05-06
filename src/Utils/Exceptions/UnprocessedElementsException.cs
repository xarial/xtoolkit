//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

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
    public class UnprocessedElementsException<T> : Exception
    {
        /// <summary>
        /// List of unprocessed elements
        /// </summary>
        public IReadOnlyList<T> UnprocessedElements { get; }

        /// <summary>
        /// Defautl constructor
        /// </summary>
        public UnprocessedElementsException(IReadOnlyList<T> unprocessedElems) : base("Unprocessed elements identified. This happens if there a circular dependency in the hierarchy or equality comparer skipping instances") 
        {
            UnprocessedElements = unprocessedElems;
        }
    }
}
