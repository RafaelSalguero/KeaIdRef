using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeaIdRef.Test
{
    public class Db : IDisposable 
    {
        public Db()
        {
            Cities = new City[]
            {
                new City { Id= 0, Name="Hermosillo" },
                new City { Id= 1, Name="Obregon" },
                new City { Id= 2, Name="Navojoa" },
            };
        }
        public IEnumerable<City> Cities { get; private set; }

        public void Dispose()
        {
        }
    }
}
