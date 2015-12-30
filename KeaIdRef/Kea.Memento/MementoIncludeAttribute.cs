using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea
{
    /// <summary>
    /// Mark a virtual property to be included as a CopyProperty by the memento factory
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class MementoIncludeAttribute : Attribute
    {
        /// <summary>
        /// Create a new memento include attribute
        /// </summary>
        public MementoIncludeAttribute()
        { }
    }
}
