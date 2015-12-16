using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
namespace Kea.Extensions.Test
{
    [TestClass]
    public class UnitTest1
    {
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
