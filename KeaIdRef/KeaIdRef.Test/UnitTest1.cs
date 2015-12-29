using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeaIdRef.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IsInCollectionSingleSelector()
        {
            //Configuration store, this could be physical files in production
            var Config = new InMemoryConfig();
            Func<Db> Db = () => new Test.Db();

            //Create a new view model:
            var VM = new SingleCityViewModel(Db, Config, false);

            //Supose that the user choose the first city:
            VM.City = VM.Cities.First();

            //Supose that the close event on the window triggers the SaveConfig method:
            VM.SaveConfig();

            //Simulate application start again:
            VM = null;

            //The user opens the window again, expecting viewing the first city as the selected one,
            //here we skip substitution so the view model will be on an incorrect state
            VM = new SingleCityViewModel(Db, Config, true);

            //The City.Id is correct but the instance is not the first element of the list
            //This view model would not be displayed correctly by a view because the selected item instance is not contained on the items source collection
            Assert.AreEqual(0, VM.City.Id);
            Assert.AreNotEqual(VM.Cities.First(), VM.City);

            //The user opens the window again, this time without skipping the substitution
            VM = new SingleCityViewModel(Db, Config, false);
            Assert.AreEqual(0, VM.City.Id);
            Assert.AreEqual(VM.Cities.First(), VM.City);
        }

        [TestMethod]
        public void IsInCollectionMultipleSelector()
        {
            //Configuration store, this could be physical files in production
            var Config = new InMemoryConfig();
            Func<Db> Db = () => new Test.Db();

            //Create a new view model:
            var VM = new MultiCityViewModel(Db, Config, false);

            //The user select the first two cities
            VM.SelectedCities.Add(VM.Cities[0]);
            VM.SelectedCities.Add(VM.Cities[1]);

            //Supose that the close event triggers the save config:
            VM.SaveConfig();
            VM = null;

            //The user opens the window (with subistitute disabled)
            VM = new MultiCityViewModel(Db, Config, true);

            //The object Ids are correct but the object references are incorrect:
            Assert.IsTrue(VM.Cities.Take(2).Select(x => x.Id).SequenceEqual(VM.SelectedCities.Select(x => x.Id)));
            Assert.IsFalse(VM.Cities.Take(2).SequenceEqual(VM.SelectedCities));

            //The user opens the window (with substitute enabled)
            VM = new MultiCityViewModel(Db, Config, false);

            Assert.IsTrue(VM.Cities.Take(2).SequenceEqual(VM.SelectedCities));
        }
    }
}
