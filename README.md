# CopperIngot
Search engine for .NET C# applications.

Allows filtering IEnumerable and IQueryable collections by constructing expression trees at runtime. 

Currently supported types for comparison: 
* All primitive types
* `string`
* `Guid`, `DateTime` and `DateOnly` as separate builders (can be added via `.WithDefaultBuilders()` at configuration stage)

`DateTime` and `DateOnly` types are currently in work.

# Configuring:
```
//You can define a custom alias. Can be used to avoid long nested property names (like Obj.Property1.Property2.Property3)
SearchAlias alias = new SearchAlias("SomeAlias").WithPropertyOf<SomeType>(nameof(SomeType.StringProperty));

SearchEngineConfiguration configuration = new SearchEngineConfiguration()
                                              //Use some custom filter builders.
                                              .WithFilterBuilder(new GuidFilterBuilder())
                                              //Use alias.
                                              .WithAlias(alias)
                                              //Disable or enable filter caching (enabled by default).
                                              //Caching is not yet supported for IQueryable filters.
                                              .UseCache(useCache)
                                              //Enable or disable aliases (disabled by default).
                                              .UseAliases();
                                              
//Search engine can be injected via some DI instrumetns or used like this.
//Note: If you are using caching option - inject it as a singleton.
ISearchEngine searchEngine = new SearchEngine(configuration);
```

`ISearchEngine` also allows re-configuring with `.Configure()` method.

Also, you can implement your own filter builders for specific types and your own custom request types. For example look at `GuidFilterBuilder.cs` and `ObjectSearchRequest.cs` implementation.

# Usage example:
```
StringSearchRequest filter
      = new(nameof(SomeType.StringProperty), //Property name or alias name
            someString, //Value
            SearchComparison.Equals, //Comparison type
            StringComparison.InvariantCultureIgnoreCase //For StringSearchRequest you can specify string comparison parameter
          );

//ISearchEngine.From() creates a new searching session that allows chaining of .Where() calls. 
IQueryable<SomeType> query = _searchEngine.From(someQueryable).Where(filter).AsQueryable();
```
