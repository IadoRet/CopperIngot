using System.Reflection;
using CopperIngot.Exceptions;
using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

internal class SearchProperty<T> : ISearchProperty
{
    private readonly PropertyInfo _propertyInfo;
    
    internal SearchProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));
        
        PropertyInfo? property = typeof(T).GetProperty(propertyName);
        _propertyInfo = property ?? throw new PropertyNotFoundException(propertyName, typeof(T));
    }
    
    public PropertyInfo GetProperty() => _propertyInfo;
}