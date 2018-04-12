using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using LibraryExtensions;
namespace UnitTesting
{
    [TestClass]
    public class UtSegmentedEnumerable
    {
        [TestMethod]
        public void TestGetNextSegment()
        {
            IEnumerable<int> values = Enumerable.Range(1, 99);
            CollectionSegmenter<int> segmenter = new CollectionSegmenter<int>(values, 0);
            IEnumerable<int> newSegment;
            int numberOfSegment = 0;
            int firstOfAllSegment = -1;
            int lastOfAllSegment = -1;
            int firstOfSixthSegment = -1;
            while ((newSegment = segmenter.GetNextSegment(10)).Any())
            {
                ++numberOfSegment;
                var newArr = newSegment.ToArray();
                firstOfSixthSegment = numberOfSegment == 6 ? newArr.First() : -1;
                firstOfAllSegment = newArr.First();
                lastOfAllSegment = newArr.Last();
            }
            Assert.AreEqual(10, numberOfSegment);
            Assert.AreEqual(1, firstOfAllSegment);
            Assert.AreEqual(51, firstOfSixthSegment);
            Assert.AreEqual(99, lastOfAllSegment);
        }
    }
}
