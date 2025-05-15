using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Exceptions;
using Xarial.XToolkit.Helpers;

namespace Utils.Tests
{
    public class HierarchyTests
    {
        public class Elem
        {
            public int Id { get; }
            public IReadOnlyList<Elem> Children { get; }

            public Elem(int id, IReadOnlyList<Elem> children)
            {
                Id = id;
                Children = children;
            }

            public override string ToString() => Id.ToString();
        }

        public class ElemEqualityComparer : IEqualityComparer<Elem>
        {
            public bool Equals(Elem x, Elem y) => x.Id == y.Id;

            public int GetHashCode(Elem obj) => 0;
        }

        [Test]
        public void OrderSimpleTest() 
        {
            //4 { 1, 3 { 1, 2 } }

            var e1 = new Elem(1, null);
            var e1_1 = new Elem(1, null);
            var e2 = new Elem(2, null);

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            var e4 = new Elem(4, new Elem[] { e1_1, e3 });

            var ordered = Hierarchy.Order(new Elem[] { e2, e1, e1_1, e4, e3 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4 }, ordered.Select(x => x.Id));
        }

        [Test]
        public void OrderCircularChildrenTest()
        {
            //4 { 1, 3 { 1, 2 { 3 } } }

            var e1 = new Elem(1, null);
            var e1_1 = new Elem(1, null);

            var e3_1 = new Elem(3, new Elem[] { e1, new Elem(2, null) });

            var e2 = new Elem(2, new Elem[] { e3_1 });

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            var e4 = new Elem(4, new Elem[] { e1_1, e3 });

            var ordered = Hierarchy.Order(new Elem[] { e2, e1, e1_1, e4, e3 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4 }, ordered.Select(x => x.Id));
        }

        [Test]
        public void OrderTest() 
        {
            //6 { 3, 4, 1}
            //11 { 8 { 5 { 1, 2 }, 1, 2, 3 }, 9 { 5 { 1, 2 }, 4, 1 }, 10 { 1, 2, 3 } }
            //7 { 1, 2 }

            var e1 = new Elem(1, null);
            var e2 = new Elem(2, null);
            var e3 = new Elem(3, null); 
            var e4 = new Elem(4, null);
            var e1_1 = new Elem(1, null);
            var e2_1 = new Elem(2, null);

            var e5 = new Elem(5, new Elem[] { e1, e2 });
            var e6 = new Elem(6, new Elem[] { e3, e4, e1_1 });
            var e7 = new Elem(7, new Elem[] { e1, e2_1 });
            var e5_1 = new Elem(5, new Elem[] { e1, e2 });

            var e8 = new Elem(8, new Elem[] { e5, e1, e2, e3 });
            var e9 = new Elem(9, new Elem[] { e5_1, e4, e1_1 });
            var e10 = new Elem(10, new Elem[] { e1, e2, e3 });

            var e11 = new Elem(11, new Elem[] { e8, e9, e10 });

            var ordered = Hierarchy.Order(new Elem[] { e4, e5, e6, e10, e1_1, e11, e2_1, e1, e3, e2, e7, e8, e5_1, e9 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 3, 4, 1, 6, 2, 5, 8, 9, 10, 11, 7 }, ordered.Select(x => x.Id));
        }

        [Test]
        public void OrderExtraItemTest()
        {
            //4 { 1, 3 { 1, 2 } }

            var e1 = new Elem(1, null);
            var e1_1 = new Elem(1, null);
            var e2 = new Elem(2, null);

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            var e4 = new Elem(4, new Elem[] { e1_1, e3 });

            var ordered = Hierarchy.Order(new Elem[] { e1, e1_1, e4, e3 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 1, 3, 4 }, ordered.Select(x => x.Id));
        }

        [Test]
        public void OrderNoRootItemsTest()
        {
            //3 { 1, 2 { 3 } }

            var e1 = new Elem(1, null);
            var e3_1 = new Elem(3, null);
            var e2 = new Elem(2, new Elem[] { e3_1 });

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            Assert.Throws<RootElementsMissingException>(() => Hierarchy.Order(new Elem[] { e1, e2, e3 }, x => x.Children, new ElemEqualityComparer()).ToArray());
        }

        [Test]
        public void OrderUnprocessedRootItemsTest()
        {
            //3 { 1, 2 { 3 } }
            //5 { 4 }

            var e1 = new Elem(1, null);
            var e3_1 = new Elem(3, null);
            var e2 = new Elem(2, new Elem[] { e3_1 });

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            var e4 = new Elem(4, null);

            var e5 = new Elem(5, new Elem[] { e4 });

            Assert.Throws<RootElementsMissingException>(() => Hierarchy.Order(new Elem[] { e1, e2, e3, e4, e5 }, x => x.Children, new ElemEqualityComparer()).ToArray());
        }

        [Test]
        public void FlattenTest()
        {
            //6 { 3, 4, 1}
            //11 { 8 { 5 { 1, 2 }, 1, 2, 3 }, 9 { 5 { 1, 2 }, 4, 1 }, 10 { 1, 2, 3 } }
            //7 { 1, 2 }

            var e1 = new Elem(1, null);
            var e2 = new Elem(2, null);
            var e3 = new Elem(3, null);
            var e4 = new Elem(4, null);
            var e1_1 = new Elem(1, null);
            var e2_1 = new Elem(2, null);

            var e5 = new Elem(5, new Elem[] { e1, e2 });
            var e6 = new Elem(6, new Elem[] { e3, e4, e1_1 });
            var e7 = new Elem(7, new Elem[] { e1, e2_1 });
            var e5_1 = new Elem(5, new Elem[] { e1, e2 });

            var e8 = new Elem(8, new Elem[] { e5, e1, e2, e3 });
            var e9 = new Elem(9, new Elem[] { e5_1, e4, e1_1 });
            var e10 = new Elem(10, new Elem[] { e1, e2, e3 });

            var e11 = new Elem(11, new Elem[] { e8, e9, e10 });

            var flat = Hierarchy.Flatten(new Elem[] { e6, e11, e7 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 6, 3, 4, 1, 11, 8, 5, 2, 9, 10, 7 }, flat.Select(x => x.Id));
        }

        [Test]
        public void FlattenCircularDependenciesItemsTest()
        {
            //3 { 1, 2 { 3 } }

            var e1 = new Elem(1, null);
            var e3_1 = new Elem(3, null);
            var e2 = new Elem(2, new Elem[] { e3_1 });

            var e3 = new Elem(3, new Elem[] { e1, e2 });

            var flat = Hierarchy.Flatten(new Elem[] { e3 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 3, 1, 2 }, flat.Select(x => x.Id));
        }

        [Test]
        public void IterateRootElementsTest() 
        {
            //6 { 3, 4, 1}
            //11 { 8 { 5 { 1, 2 }, 1, 2, 3 }, 9 { 5 { 1, 2 }, 4, 1 }, 10 { 1, 2, 3 } }
            //7 { 1, 2 }

            var e1 = new Elem(1, null);
            var e2 = new Elem(2, null);
            var e3 = new Elem(3, null);
            var e4 = new Elem(4, null);
            var e1_1 = new Elem(1, null);
            var e2_1 = new Elem(2, null);

            var e5 = new Elem(5, new Elem[] { e1, e2 });
            var e6 = new Elem(6, new Elem[] { e3, e4, e1_1 });
            var e7 = new Elem(7, new Elem[] { e1, e2_1 });
            var e5_1 = new Elem(5, new Elem[] { e1, e2 });

            var e8 = new Elem(8, new Elem[] { e5, e1, e2, e3 });
            var e9 = new Elem(9, new Elem[] { e5_1, e4, e1_1 });
            var e10 = new Elem(10, new Elem[] { e1, e2, e3 });

            var e11 = new Elem(11, new Elem[] { e8, e9, e10 });

            var roots = Hierarchy.IterateRootElements(new Elem[] { e4, e5, e6, e10, e1_1, e11, e2_1, e1, e3, e2, e7, e8, e5_1, e9 }, x => x.Children, new ElemEqualityComparer()).ToArray();

            CollectionAssert.AreEqual(new int[] { 6, 11, 7 }, roots.Select(x => x.Id));
        }
    }
}
