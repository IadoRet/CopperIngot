using CopperIngot.Enums;
using CopperIngot.Interfaces;

namespace CopperIngot.Requests;

public readonly struct StringSearchRequest(
    string property,
    string value,
    SearchComparison comparison,
    StringComparison stringComparison = StringComparison.Ordinal) : ISearchRequest
{
    public SearchComparison Comparison { get; } = comparison;

    public StringComparison StringComparison { get;  } = stringComparison;

    public string Property { get; } = property;

    public object GetValue() => value;

    public int GetUniqueKey()
    {
        unchecked
        {
            return Comparison.GetHashCode() + StringComparison.GetHashCode() + Property.GetHashCode();
        }
    }
}