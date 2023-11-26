using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

internal class CachedFilter<T>(Func<T, object?, bool> filter, Type comparableType) : ICachedFilter
{
    public Type ComparableType { get; } = comparableType;

    public IEnumerable<T> Where(IEnumerable<T> values, object? filterValue)
    {
        return values.Where(v => filter(v, filterValue));
    }

    public T? FirstOrDefault(IEnumerable<T> values, object? filterValue)
    {
        return values.FirstOrDefault(v => filter(v, filterValue));
    }
}