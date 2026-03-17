using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PagePacker
{
    /// <summary>
    /// This class encapsulates a pair of integers representing the start and end of an interval.
    /// </summary>
    public struct Interval
    {
        public int IntervalStart;
        public int IntervalEnd;

        public Interval(int start, int end)
        {
            this.IntervalStart = start;
            this.IntervalEnd = end;
        }
    }

    /// <summary>
    /// This class encapsulates a pair of integers representing the start and end of an interval (such as a substring within a string), as 
    /// well as another integer used to represent the index of an item in a collection.
    /// </summary>
    public struct IndexingInterval
    {
        /// <summary>
        /// The start point of the interval.
        /// </summary>
        public int iStart;

        /// <summary>
        /// The end point of the interval.
        /// </summary>
        public int iEnd;

        /// <summary>
        /// The index of an item within a collection which this interval is associated with.
        /// </summary>
        public int iIndex;

        public IndexingInterval(int start, int end, int itemIndex)
        {
            this.iStart = start;
            this.iEnd = end;
            this.iIndex = itemIndex;
        }
    }
}
