using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Interfaces
{
    /// <summary>
    /// A service that gets and sets values by string keys
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Set a value by its key
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        void Set(string Key, string Value);
        /// <summary>
        /// Gets a value by its key, returns null if not found
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        string Get(string Key);
    }
}
