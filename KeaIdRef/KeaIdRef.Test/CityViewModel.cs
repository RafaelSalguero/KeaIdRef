using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kea.Serialization;

namespace KeaIdRef.Test
{
    /// <summary>
    /// A view model that contains all cities
    /// </summary>
    public class CityViewModel
    {
        public CityViewModel (Func<Db> Db, IConfig Config, bool SkipSubstitute)
        {
            this.Config = Config;
            using (var C = Db())
            {
                Cities = C.Cities.ToList();
            }
            LoadConfig(SkipSubstitute);
        }
        readonly IConfig Config;

        /// <summary>
        /// Item source
        /// </summary>
        public IReadOnlyList<City> Cities { get; private set; }

        /// <summary>
        /// Save the current view model state onto the configuration store
        /// </summary>
        public void SaveConfig()
        {
            Config.Save(Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
         void LoadConfig(bool SkipSubstitute)
        {
            //Load view model data from config:
            var data = Config.Load();
            if (data != null)
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(data, this);

                if (!SkipSubstitute)
                    IsContainedInAttribute.Substitute(this);
            }
        }

    }
}
