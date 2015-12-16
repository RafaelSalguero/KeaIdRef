using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

namespace Kea.Serialization
{
    /// <summary>
    /// Indicate that the value of this property is contained on the given collection
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IsContainedInAttribute : Attribute
    {
        readonly string collectionProperty;

        /// <summary>
        /// Indicate that the value of this property is contained on the given collection
        /// </summary>
        /// <param name="collectionProperty">The name of the collection property of this class that contains this property value</param>
        /// <param name="idProperty">The name of the property of the property value that will be used to compare against the collection items</param>
        public IsContainedInAttribute(string collectionProperty, string idProperty)
        {
            this.collectionProperty = collectionProperty;
            this.idProperty = idProperty;
        }

        /// <summary>
        /// The name of the collection property that contains this property value
        /// </summary>
        public string CollectionProperty
        {
            get { return collectionProperty; }
        }

        readonly string idProperty;
        public string IdProperty
        {
            get
            {
                return idProperty;
            }
        }

        [ThreadStatic]
        static Dictionary<Tuple<Type, string>, Func<object, object>> GetIdPropertyCache;
        /// <summary>
        /// Compile an expression tree that gets the IdProperty of the item
        /// </summary>
        /// <returns></returns>
        static Func<object, object> GetIdPropertyFunc(Type ElementType, string IdProperty)
        {
            var Params = Tuple.Create(ElementType, IdProperty);
            if (GetIdPropertyCache == null)
                GetIdPropertyCache = new Dictionary<Tuple<Type, string>, Func<object, object>>();

            Func<object, object> Result;
            if (!GetIdPropertyCache.TryGetValue(Params, out Result))
            {
                var Param = Expression.Parameter(typeof(object));
                var Item = Expression.Convert(Param, ElementType);
                var Getter = Expression.Convert(Expression.Property(Item, IdProperty), typeof(object));
                var Func = Expression.Lambda<Func<object, object>>(Getter, Param);

                Result = Func.Compile();
                GetIdPropertyCache.Add(Params, Result);
            }
            return Result;
        }


        /// <summary>
        /// Substitute values from properties that have the IsContainedIn attribute with the instance of the value of the collection
        /// </summary>
        /// <param name="Instance">The object instante that contains the properties to substitute</param>
        public static void Substitute(object Instance)
        {
            var Type = Instance.GetType();
            var Props = Type.GetProperties();
            foreach (var P in Props)
            {
                var Att = P.GetCustomAttribute<IsContainedInAttribute>();
                if (Att != null)
                {
                    //Get the collection:
                    var Collection = Type.GetProperty(Att.CollectionProperty);
                    if (Collection == null)
                        throw new ArgumentException($"The property {P.Name} have the collection property {Att.CollectionProperty} which was not found on class {Type.FullName}");

                    var CollectionValue = Collection.GetValue(Instance);
                    if (CollectionValue == null )
                        throw new ArgumentException($"The property {P.Name} have the collection property {Att.CollectionProperty} which is null");


                    //Check if collections is IEnumerable:
                    var AsEnumerable = CollectionValue as IEnumerable;
                    if (AsEnumerable == null)
                        throw new ArgumentException($"The property {P.Name} have the collection property {Att.CollectionProperty} which is not an IEnumerable");

                    //Search for the item with the same IdProperty on the collection:
                    var PropGetter = GetIdPropertyFunc(P.PropertyType, Att.IdProperty);
                    var PropValue = P.GetValue(Instance);
                    if (PropValue != null)
                    {
                        var IdValue = PropGetter(PropValue);
                        var Reference = AsEnumerable.Cast<object>()
                            .Where(x => x != null)
                            .Where(x => object.Equals(PropGetter(x), IdValue))
                            .FirstOrDefault();

                        if (Reference == null)
                            throw new ArgumentException($"The collection {Att.CollectionProperty} from the class {Type.FullName} does not contain the value for the property {P.Name }");

                        //Set property equal to the collection instance:
                        P.SetValue(Instance, Reference);
                    }
                }
            }
        }
    }
}
