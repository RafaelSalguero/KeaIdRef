using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea
{
    /// <summary>
    /// Provides LINQ like methods for manipulating untyped enumerables
    /// </summary>
    public static class RLinq
    {
        /// <summary>
        /// Gets whether a collection implements the ICollection(T) interface 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static bool IsICollectionOfT(IEnumerable Collection)
        {
            return Collection.GetType().IsSubClassOfGeneric(typeof(ICollection<>));
        }
        /// <summary>
        /// Gets whether a collection implements the ICollection(T) interface 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static bool IsICollectionOfT(Type CollectionType)
        {
            return CollectionType.IsSubClassOfGeneric(typeof(ICollection<>));
        }

        /// <summary>
        /// Gets whether a collection implements the ICollection(T) interface 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static bool IsIEnumerableOfT(Type CollectionType)
        {
            return CollectionType.IsSubClassOfGeneric(typeof(IEnumerable<>));
        }

        /// <summary>
        /// Gets the first type argument of a generic IEnumerable 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static Type GetEnumerableType(this IEnumerable Collection)
        {
            var collectionType = Collection.GetType();
            var IEnumerable =
                collectionType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .First();

            return IEnumerable.GetGenericArguments()[0];
        }

        /// <summary>
        /// Gets the first type argument of a generic IEnumerable 
        /// </summary>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static Type GetEnumerableType(Type collectionType)
        {
            Func<Type, bool> predicate = x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            if (predicate(collectionType))
                return collectionType.GetGenericArguments()[0];

            var IEnumerable =
                collectionType
                .GetInterfaces()
                .Where(predicate)
                .First();

            return IEnumerable.GetGenericArguments()[0];
        }
    }
}
