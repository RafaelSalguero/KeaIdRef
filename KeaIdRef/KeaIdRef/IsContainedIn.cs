using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="collectionProperty"></param>
        public IsContainedInAttribute(string collectionProperty)
        {
            this.collectionProperty = collectionProperty;
        }

        /// <summary>
        /// The name of the collection property that contains this property value
        /// </summary>
        public string CollectionProperty
        {
            get { return collectionProperty; }
        }
    }
}
