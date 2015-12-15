using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kea.Interfaces
{
    /// <summary>
    /// Implements an in-memory dictionary as IConfig service 
    /// </summary>
    public class InMemoryConfig : IConfig
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        string IConfig.Get(string Key)
        {
            string result;
            if (data.TryGetValue(Key, out result))
                return result;
            else
                return null;
        }

        void IConfig.Set(string Key, string Value)
        {
            data[Key] = Value;
        }
    }
}
