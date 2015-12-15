using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeaIdRef.Test
{
    /// <summary>
    /// Gets and sets string values
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Set a value by its key
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        void Save(string Value);
        /// <summary>
        /// Gets a value by its key, returns null if not found
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        string Load();
    }

    /// <summary>
    /// Provides an in memory configuration store, on production, there would be an implementation that write to files
    /// </summary>
    public class InMemoryConfig : IConfig
    {
        private string Value;
        public string Load()
        {
            return Value;
        }

        public void Save(string Value)
        {
            this.Value = Value;
        }
    }
}
