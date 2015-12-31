using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static Kea.Extensions.LinqExtensions;

namespace Kea.Extensions.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SelectPos()
        {
            Assert.AreEqual(0, new int[0].SelectLookahead(Tuple.Create).Count());

            Assert.IsTrue(new[] { 1, 2, 3, 4, 5 }.SelectLookahead(Tuple.Create).SequenceEqual(
               new[]
               {
                    Tuple.Create (0, 1, 2, 0 , SequencePosition.First ),
                    Tuple.Create (1, 2, 3, 1 , SequencePosition.Middle ),
                    Tuple.Create (2, 3, 4, 2 , SequencePosition.Middle ),
                    Tuple.Create (3, 4, 5, 3 , SequencePosition.Middle ),
                    Tuple.Create (4, 5, 0, 4 , SequencePosition.Last ),
               }));

            Assert.IsTrue(new[] { 1, 2, 3 }.SelectLookahead(Tuple.Create).SequenceEqual(
               new[]
               {
                    Tuple.Create (0, 1, 2, 0 , SequencePosition.First ),
                    Tuple.Create (1, 2, 3, 1 , SequencePosition.Middle ),
                    Tuple.Create (2, 3, 0, 2 , SequencePosition.Last ),
               }));

            Assert.IsTrue(new[] { 1, 2
            }.SelectLookahead(Tuple.Create).SequenceEqual(
               new[]
               {
                    Tuple.Create (0, 1, 2, 0 , SequencePosition.First ),
                    Tuple.Create (1, 2, 0, 1 , SequencePosition.Last ),
               }));

            Assert.IsTrue(new[] { 1
            }.SelectLookahead(Tuple.Create).SequenceEqual(
               new[]
               {
                    Tuple.Create (0, 1, 0, 0 , SequencePosition.Single ),
               }));
        }

        [TestMethod]
        public void SequenceCompareTest()
        {
            Assert.IsTrue(new[] { 1, 2, 3, 4 }.SequenceEqual(new[] { 1, 2, 3, 4 }, (a, b) => a == b));

            Assert.IsFalse(new[] { 1, 2, 3, 4, 5 }.SequenceEqual(new[] { 1, 2, 3, 4 }, (a, b) => a == b));
            Assert.IsFalse(new[] { 1, 2, 3, 4 }.SequenceEqual(new[] { 1, 2, 3, 4, 5 }, (a, b) => a == b));

            Assert.IsFalse(new[] { 1, 2, 3, 4 }.SequenceEqual(new[] { 2, 2, 3, 4 }, (a, b) => a == b));
            Assert.IsFalse(new[] { 1, 2, 3, 5 }.SequenceEqual(new[] { 1, 2, 3, 4 }, (a, b) => a == b));

            Assert.IsTrue(new[] { 1 }.SequenceEqual(new[] { 1 }, (a, b) => a == b));
            Assert.IsTrue(new int[0].SequenceEqual(new int[0], (a, b) => a == b));

        }

        [TestMethod]
        public void SplitTest()
        {
            var items = new[]
            {
                10, 12, 1, 2, 3, 4, 1, 5 ,6 , 1,7
            };

            var expected = new[]
            {
                new [] { 10, 12 },
                new [] { 2, 3, 4 },
                new [] { 5, 6},
                new [] { 7 },
            };

            Func<IEnumerable<IEnumerable<int>>, IEnumerable<IEnumerable<int>>, bool> SeqEq = (a, b) => a.SequenceEqual(b, (c, d) => c.SequenceEqual(d));

            Assert.IsTrue(SeqEq(expected, items.Split(x => x == 1)));
        }

        [TestMethod]
        public void AggregareAdjacentsTest()
        {
            //Aggregate all adjacent even numbers:
            var items = new[]
            {
                1,
                3,
                2,
                3,
                4,
                6,
                7,
                10,
                2,
                2,
                4,
                5,
            };


            Func<IEnumerable<int>, IEnumerable<int>> F = Items => Items.AggregateAdjacents((a, b) => a % 2 == 0 && b % 2 == 0, (a, b) => a + b);

            Assert.IsTrue(new[]
            {
                1,
                3,
                2,
                3,
                10,
                7,
                18,
                5,
            }.SequenceEqual(F(new[]
            {
                1,
                3,
                2,
                3,
                4,
                6,
                7,
                10,
                2,
                2,
                4,
                5,
            })));

            Assert.IsTrue(new[]
           {
                1,
                3,
                2,
                3,
                10,
                7,
                18,
            }.SequenceEqual(F(new[]
           {
                1,
                3,
                2,
                3,
                4,
                6,
                7,
                10,
                2,
                2,
                4,
            })));

            Assert.IsTrue(new[]
          {
                10,
            }.SequenceEqual(F(new[]
          {
                2,
                2,
                6
            })));
        }

        [TestMethod]
        public void SyncTest()
        {
            List<int> Dest = new List<int>();

            Dest.Add(1);
            Dest.Add(2);

            var Source = new[] { 1, 2, 3, 4 };

            Source.PopulateList(Dest);

            Assert.IsTrue(Source.SequenceEqual(Dest));

            Source = new[] { 1, 2 };

            Source.PopulateList(Dest);
            Assert.IsTrue(Source.SequenceEqual(Dest));
        }

        [TestMethod]
        public void SelectPreviousTest()
        {
            var Items = new[] { 1, 2, 3, 4, 5 };

            var Expected = new[]
            {
                Tuple.Create (2, 1),
                Tuple.Create (3, 2),
                Tuple.Create (4, 3),
                Tuple.Create (5, 4),
            };

            var Actual = Items.SelectPrevious(Tuple.Create);
            Assert.IsTrue(Actual.SequenceEqual(Expected));
        }
    }
}
