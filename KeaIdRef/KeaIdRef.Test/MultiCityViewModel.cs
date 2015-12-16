using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kea.Serialization;

namespace KeaIdRef.Test
{
    /// <summary>
    /// A view model that can be binded onto a multiple item selector (check list box, ...)
    /// </summary>
    public class MultiCityViewModel : CityViewModel
    {
        /// <summary>
        /// Create a new cities view model
        /// </summary>
        /// <param name="Db">Mocked dependency to the database</param>
        /// <param name="Config">Contains the store that saves the view model configuration</param>
        /// <param name="SkipSubstitute">True to skip attribute substitution</param>
        public MultiCityViewModel(Func<Db> Db, IConfig Config, bool SkipSubstitute) : base(Db, Config, SkipSubstitute)
        {
        }


        [IsContainedIn(nameof(Cities), nameof(City.Id))]
        public List<City> SelectedCities { get; set; } = new List<City>();
    }
}
