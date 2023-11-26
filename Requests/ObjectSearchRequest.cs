using CopperIngot.Enums;
using CopperIngot.Interfaces;

namespace CopperIngot.Requests;

public readonly struct ObjectSearchRequest(
    string property, 
    object value, 
    SearchComparison comparison) : ISearchRequest
{
    public SearchComparison Comparison { get; } = comparison;

    public string Property { get; } = property;
    
    public object GetValue() => value;
    
    public int GetUniqueKey()
    {
        unchecked
        {
            return Comparison.GetHashCode() + Property.GetHashCode();
        }
    }
}