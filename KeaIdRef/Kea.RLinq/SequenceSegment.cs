using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Extensions
{
    /// <summary>
    /// Like ArraySegment but for any IReadOnlyList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    struct SequenceSegment<T> : IReadOnlyList<T>
    {
        public SequenceSegment(IReadOnlyList<T> original, int offset, int count)
        {
            if (offset < 0 || offset >= original.Count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if ((count + offset) > original.Count)
                throw new ArgumentOutOfRangeException(nameof(count));


            this.Original = original;
            this.offset = offset;
            this.count = count;
        }
        IReadOnlyList<T> Original;
        readonly int offset, count;

        public T this[int index]
        {
            get
            {
                if (index >= count || index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Original[index + offset];
            }
        }

        public int Count => count;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = offset; i < offset + count; i++)
                yield return Original[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
