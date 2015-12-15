# KeaIdRef
Anotate C# properties that its value is contained on a collection

This library is intended for easily serializing view models, so a view state (ex. a window) can be saved

**The problem with ItemSources (Or any other binded collection)**

Imagine that you want to create a view that has a ComboBox wich contains a list of cities,
your view model probably will look like this

````C#
public class CitiesViewModel
{
  public CitiesViewModel(Func<Db> Db)
  {
      //Initialize cities from a database
      using (var C = Db())
      {
        Cities = C.Cities.ToList();
      }
  }

  //Contains all cities available to the user
  public IEnumerable<City> Cities { get; private set }
  
  //Selected city
  public City City { get; set; }
}
````
If you want to serialize the view model state so each time the user enter the cities window it have the same values you would have to save the *reference* of the city inside the `Cities` property instead of the City object.

####The `IsContainedIn` attribute
Anotating a property with this attribute implies that its value its contained on a collection, and the attribute provides an static method for solving the property value *after* the deserialization have passed.

````C#
  //Contains all cities available to the user
  public IEnumerable<City> Cities { get; private set }
  
  //Selected city
  [IsContainedIn (nameof(Cities), nameof(City.Id))]
  public City City { get; set; }
````

Here we asume that the class `City` have a property `Id` that will be our *IdProperty*, this is the property that will be used for searching instances on the `Cities` collection

After loading again the `Cities` collection and deserializing the view model, the property `City` will be assigned to an instance of the entity that it isn't contained on the collection, then we call the `IsContainedIn.Substitute(ViewModel)`, and the `City` property value will be substituted for the item instance of the `Cities` collection that have the same `Id`

This way we can serialize and deserialize a view model that contains binding properties for collection controls such as lists or combos