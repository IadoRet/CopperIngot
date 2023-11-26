using System.Reflection;
using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

/// <summary>
/// Alias for property.
/// Can be used to avoid passing long property paths (like Obj.Prop1.Prop2.Prop3)
/// </summary>
/// <param name="aliasName"></param>
public class SearchAlias(string aliasName)
{
    private readonly List<ISearchProperty> _properties = [];

    /// <summary>
    /// With property of type T.
    /// Chain calls for each nested property.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <typeparam name="T">Type which contains specified property</typeparam>
    /// <returns>This instance</returns>
    public SearchAlias WithPropertyOf<T>(string propertyName)
    {
        _properties.Add(new SearchProperty<T>(propertyName));
        return this;
    }
    
    internal string GetName() => aliasName;

    internal PropertyInfo[] GetProperties() => _properties.Select(p => p.GetProperty()).ToArray();
}