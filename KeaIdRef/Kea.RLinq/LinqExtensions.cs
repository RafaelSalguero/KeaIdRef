using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Extensions
{
    /// <summary>
    /// Linq extensions
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// The relative position of an element in a sequence
        /// </summary>
        public enum SequencePosition
        {
            /// <summary>
            /// This is the first element of a sequence of two or more elements. The 'last' element is default(T) on this position
            /// </summary>
            First,
            /// <summary>
            /// This is an element in the middle of a sequence of three or more elements. All 'last', 'current' and 'next' elements have an assigned value on this position
            /// </summary>
            Middle,

            /// <summary>
            /// This is the last element of a sequence of two or more elements. The 'next' element is default(T) on this position
            /// </summary>
            Last,

            /// <summary>
            /// This is the single element of a one element sequence. The 'current' element is the only one that have an assigned value on this position
            /// </summary>
            Single
        }

        /// <summary>
        /// Create a segment of a list without copying each element of the list or creating a new instance of a list or an array
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="items">Original items</param>
        /// <param name="offset">Segment offset</param>
        /// <param name="count">Segment size</param>
        /// <returns>An implementation of the IReadOnlyList interface that mantains a reference to the original list</returns>
        public static IReadOnlyList<T> Segment<T>(this IReadOnlyList<T> items, int offset, int count)
        {
            return new SequenceSegment<T>(items, offset, count);
        }

        /// <summary>
        /// Apply a selector to a function that have access to the previews, current, and next item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Items"></param>
        /// <param name="Selector">Function that takes (last, current, next, index, position) parameters and return the select result</param>
        /// <returns></returns>
        public static IEnumerable<TResult> SelectLookahead<T, TResult>(this IEnumerable<T> Items, Func<T, T, T, int, SequencePosition, TResult> Selector)
        {
            T Last = default(T);
            T Current = default(T);

            int index = 0;
            foreach (var Next in Items)
            {
                if (index >= 2)
                {
                    yield return Selector(Last, Current, Next, index - 1, SequencePosition.Middle);
                }
                else if (index >= 1)
                {
                    yield return Selector(default(T), Current, Next, index - 1, SequencePosition.First);
                }

                index++;
                Last = Current;
                Current = Next;
            }
            if (index == 1)
                yield return Selector(default(T), Current, default(T), 0, SequencePosition.Single);
            else if (index > 1)
                yield return Selector(Last, Current, default(T), index - 1, SequencePosition.Last);

        }


        /// <summary>
        /// Checks if a sequence equals to other using the given comparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <param name="Other">Sequence to compare</param>
        /// <param name="Comparer">Item wise comparer</param>
        public static bool SequenceEqual<T>(this IEnumerable<T> Items, IEnumerable<T> Other, Func<T, T, bool> Comparer)
        {
            var ItemsEnum = Items.GetEnumerator();
            var OtherEnum = Other.GetEnumerator();

            while (ItemsEnum.MoveNext())
            {
                bool Ot = OtherEnum.MoveNext();
                //Other have fewer elements:
                if (Ot == false)
                    return false;

                if (!Comparer(ItemsEnum.Current, OtherEnum.Current))
                    return false;
            }

            //Items have fewer element:
            if (OtherEnum.MoveNext())
                return false;

            return true;
        }

        /// <summary>
        /// Split a collection by elements which are passed as separators. Separators are not included. Empty items are ignored
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <param name="IsSeparator">Separator predicate</param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this IEnumerable<T> Items, Func<T, bool> IsSeparator)
        {
            return Items.Split(IsSeparator, true);
        }

        /// <summary>
        /// Split a collection by elements which are passed as separators. Separators are not included 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <param name="IsSeparator">Separator predicate</param>
        /// <param name="IgnoreEmptyItems ">True to ignore empty items</param>
        /// <returns></returns>
        public static IEnumerable<IReadOnlyList<T>> Split<T>(this IEnumerable<T> Items, Func<T, bool> IsSeparator, bool IgnoreEmptyItems)
        {
            var Current = new List<T>();
            foreach (var It in Items)
            {
                if (IsSeparator(It))
                {
                    if (Current.Count > 0 || !IgnoreEmptyItems)
                    {
                        yield return Current;
                    }
                    Current = new List<T>();
                }
                else
                    Current.Add(It);
            }
            if (Current.Count > 0 || !IgnoreEmptyItems)
            {
                yield return Current;
            }
        }

        /// <summary>
        /// Agregate groups of items that are adjacent with the given selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Items"></param>
        /// <param name="IsAdjacent">True if two items are considered adjacents</param>
        /// <param name="Selector">Groupping selector</param>
        /// <returns></returns>
        public static IEnumerable<T> AggregateAdjacents<T>(this IEnumerable<T> Items, Func<T, T, bool> IsAdjacent, Func<T, T, T> Selector)
        {
            bool First = true;
            T Last = default(T);
            foreach (var It in Items)
            {
                if (!First)
                {
                    if (IsAdjacent(Last, It))
                        Last = Selector(Last, It);
                    else
                    {
                        yield return Last;
                        Last = It;
                    }
                }
                else
                {
                    Last = It;
                }
                First = false;
            }
            yield return Last;
        }


        /// <summary>
        /// Devuelve una cadena con todos los elementos separados
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos</typeparam>
        /// <param name="items">Elementos</param>
        /// <param name="separator">Separador entre elementos</param>
        public static string SequenceToString<T>(this IEnumerable<T> items, string separator)
        {
            StringBuilder B = new StringBuilder();
            bool first = true;
            foreach (var It in items)
            {
                if (!first)
                    B.Append(separator);
                B.Append(It.ToString());
                first = false;
            }
            return B.ToString();
        }


        /// <summary>
        /// Performs a select where the selector function have access to the previous element in the collection.
        /// Because the previous element is consumed by the selector, the resulting collection contains n-1 elements
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="Collection">Collection</param>
        /// <param name="Selector">Selector function, the first parameter is the current element and the second parameter the previous element.</param>
        public static IEnumerable<TResult> SelectPrevious<T, TResult>(this IEnumerable<T> Collection, Func<T, T, TResult> Selector)
        {
            bool first = true;
            T Previous = default(T);
            foreach (var Item in Collection)
            {
                if (!first)
                    yield return Selector(Item, Previous);
                first = false;
                Previous = Item;
            }
        }

        /// <summary>
        /// Modify the Dest collection so its equal to Source collection
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="Source">Source colllection</param>
        /// <param name="Dest">Desc collection </param>
        public static void PopulateList<T>(this IEnumerable<T> Source, IList<T> Dest)
        {
            int index = 0;
            foreach (var S in Source)
            {
                if (index < Dest.Count)
                {
                    if (!object.Equals(S, Dest[index]))
                    {
                        Dest[index] = S;
                    }
                }
                else
                    Dest.Add(S);
                index++;
            }
            //Trim dest:
            while (Dest.Count > index)
                Dest.RemoveAt(Dest.Count - 1);
        }

    }
}
