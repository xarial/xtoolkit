//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Exceptions;

namespace Xarial.XToolkit.Helpers
{
    /// <summary>
    /// Helper function to process hierarchical data
    /// </summary>
    public static class Hierarchy
    {
        /// <summary>
        /// Flattens the hierarchical structure
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Source elements</param>
        /// <param name="childrenSelector">Function to retrieve children of an element</param>
        /// <param name="comparer">Elements comparer</param>
        /// <returns>Flattened enumerable</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<T> Flatten<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
        {
            if (source == null) 
            {
                throw new ArgumentNullException(nameof(source));
            }

            return FlattenChildren(source, childrenSelector, comparer, new HashSet<T>());
        }

        /// <inheritdoc/>
        public static IEnumerable<T> Flatten<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
            => Flatten(source, childrenSelector, EqualityComparer<T>.Default);

        /// <inheritdoc/>
        public static IEnumerable<T> Flatten<T>(T source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
            => Flatten(ToEnumerable(source), childrenSelector, comparer);

        /// <inheritdoc/>
        public static IEnumerable<T> Flatten<T>(T source, Func<T, IEnumerable<T>> childrenSelector)
            => Flatten(ToEnumerable(source), childrenSelector);

        /// <summary>
        /// Iterates root elements from the source enumerable
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Source elements</param>
        /// <param name="childrenSelector">Function to retrieve children of an element</param>
        /// <param name="comparer">Elements comparer</param>
        /// <returns>Root elements (elements that are not found in the chidren tree)</returns>
        public static IEnumerable<T> IterateRootElements<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer) 
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var allChildren = new HashSet<T>(source.SelectMany(item => childrenSelector.Invoke(item) ?? Enumerable.Empty<T>()));
            return source.Where(item => !allChildren.Contains(item, comparer)).ToArray();
        }

        /// <inheritdoc/>
        public static IEnumerable<T> IterateRootElements<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
            => IterateRootElements<T>(source, childrenSelector, EqualityComparer<T>.Default);

        /// <inheritdoc/>
        public static IEnumerable<T> IterateRootElements<T>(T source, Func<T, IEnumerable<T>> childrenSelector)
            => IterateRootElements<T>(ToEnumerable(source), childrenSelector);

        /// <inheritdoc/>
        public static IEnumerable<T> IterateRootElements<T>(T source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
            => IterateRootElements<T>(ToEnumerable(source), childrenSelector, comparer);

        /// <summary>
        /// Orders elements in the hierarchical order based on the dependencies (from children to parents)
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Source elements</param>
        /// <param name="childrenSelector">Function to retrieve children of an element</param>
        /// <param name="comparer">Elements comparer</param>
        /// <returns>Elements in the order based on dependency</returns>
        /// <exception cref="RootElementsMissingException">No root elements found</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<T> Order<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var visited = new HashSet<T>();

            foreach (var root in IterateRootElements(source, childrenSelector, comparer))
            {
                foreach (var elem in OrderChildren(root, visited, childrenSelector, comparer)) 
                {
                    if (source.Contains(elem, comparer)) 
                    {
                        yield return elem;
                    }
                }
            }

            if (source.Except(visited, comparer).Any())
            {
                throw new RootElementsMissingException();
            }
        }

        /// <inheritdoc/>
        public static IEnumerable<T> Order<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
            => Order(source, childrenSelector, EqualityComparer<T>.Default);

        /// <inheritdoc/>
        public static IEnumerable<T> Order<T>(T source, Func<T, IEnumerable<T>> childrenSelector)
            => Order(ToEnumerable(source), childrenSelector, EqualityComparer<T>.Default);

        /// <inheritdoc/>
        public static IEnumerable<T> Order<T>(T source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
            => Order(ToEnumerable(source), childrenSelector, comparer);

        private static IEnumerable<T> OrderChildren<T>(T item, HashSet<T> visited, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer)
        {
            if (!visited.Contains(item, comparer))
            {
                visited.Add(item);

                var children = childrenSelector.Invoke(item) ?? Enumerable.Empty<T>();

                foreach (var child in children)
                {
                    foreach (var grandChild in OrderChildren(child, visited, childrenSelector, comparer)) 
                    {
                        yield return grandChild;
                    }
                }

                yield return item;
            }
        }

        private static IEnumerable<T> FlattenChildren<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector, IEqualityComparer<T> comparer, HashSet<T> visited)
        {
            foreach (var elem in source) 
            {
                if (!visited.Contains(elem, comparer))
                {
                    visited.Add(elem);

                    yield return elem;

                    var children = childrenSelector.Invoke(elem);

                    foreach (var child in FlattenChildren(children ?? Enumerable.Empty<T>(), childrenSelector, comparer, visited))
                    {
                        yield return child;
                    }
                }
            }
        }

        private static IEnumerable<T> ToEnumerable<T>(T item)
        {
            yield return item;
        }
    }
}
