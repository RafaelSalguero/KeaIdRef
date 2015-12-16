using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kea.Serialization;

namespace KeaIdRef.Test
{
    /// <summary>
    /// A view model that can be binded onto a single item selector (list box, combo box)
    /// </summary>
    public class SingleCityViewModel : CityViewModel
    {
        /// <summary>
        /// Create a new cities view model
        /// </summary>
        /// <param name="Db">Mocked dependency to the database</param>
        /// <param name="Config">Contains the store that saves the view model configuration</param>
        /// <param name="SkipSubstitute">True to skip attribute substitution</param>
        public SingleCityViewModel(Func<Db> Db, IConfig Config, bool SkipSubstitute) : base(Db, Config, SkipSubstitute)
        {
        }

        [IsContainedIn(nameof(Cities), nameof(Test.City.Id))]
        public City City { get; set; }
    }
}
