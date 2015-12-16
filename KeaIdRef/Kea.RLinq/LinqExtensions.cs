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
