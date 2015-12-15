using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kea.Serialization;

namespace KeaIdRef.Test
{
    public class CitiesViewModel
    {
        /// <summary>
        /// Create a new cities view model
        /// </summary>
        /// <param name="Db">Mocked dependency to the database</param>
        /// <param name="Config">Contains the store that saves the view model configuration</param>
        /// <param name="SkipSubstitute">True to skip attribute substitution</param>
        public CitiesViewModel(Func<Db> Db, IConfig Config, bool SkipSubstitute)
        {
            this.Config = Config;
            using (var C = Db())
            {
                Cities = C.Cities;
            }
            LoadConfig(SkipSubstitute);
        }
        /// <summary>
        /// Configuration store used for saving this viewmodel
        /// </summary>
        readonly IConfig Config;

        /// <summary>
        /// Save the current view model state onto the configuration store
        /// </summary>
        public void SaveConfig()
        {
            Config.Save(Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
        public void LoadConfig(bool SkipSubstitute)
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

        public IEnumerable<City> Cities { get; private set; }

        [IsContainedIn(nameof(Cities), nameof(Test.City.Id))]
        public City City { get; set; }
    }
}
