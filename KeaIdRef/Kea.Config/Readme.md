#Kea.Config

Provides an interface for a configuration service, this interface is widely used on all Kea projects and its the de-facto standard for abstracting configuration implementation from usage

###Usage

Classes can depend on the `IConfig` interface, that provides the `string Get()` and `void Set(string)` methods

Extension methods for this interface provide an easy way to save serialized objects onto any IConfig service

Because of the small size of the interface, the IConfig service is easily implemented 

**Example:**
````C#
public class Controller
{
	public Controller (IConfig Config)
	{
		this.config = Config;

		//The class can load configurations here:
		this.Title = Config.Get(nameof(Title));

		//Extension method used for loading serialized complex objects:
		this.Config = Config.Get<ConfigObject>();
	}

	readonly IConfig config;

	public string Title { get; set; }

	public ConfigObject Config { get; set;}
}
````

**Included implementations:**
`InMemoryConfig` The IConfig service implemented as a `Dictionary<string,string>`
