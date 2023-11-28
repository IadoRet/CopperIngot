using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

/// <summary>
/// Search session for chaining filtering.
/// </summary>
/// <param name="enumerable">Enumerable to filter</param>
/// <param name="searchEngine">Search engine instance</param>
/// <typeparam name="T">Type of the collection</typeparam>
public class EnumerableSearchSession<T>(IEnumerable<T> enumerable, SearchEngine searchEngine)
{
    /// <summary>
    /// Default .Where() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>This instance. Use .AsEnumerable() to get filtered query</returns>
    public EnumerableSearchSession<T> Where(ISearchRequest searchRequest)
    {
        enumerable = searchEngine.Where(enumerable, searchRequest);
        return this;
    }

    /// <summary>
    /// Default .Select() behavior
    /// </summary>
    /// <param name="func">Selector function</param>
    /// <returns>A new session instance</returns>
    public EnumerableSearchSession<TNew> Select<TNew>(Func<T, TNew> func)
    {
        return searchEngine.From(enumerable.Select(func));
    }

    /// <summary>
    /// Default .FirstOrDefault() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>First match or null</returns>
    public T? FirstOrDefault(ISearchRequest searchRequest)
    {
        return searchEngine.FirstOrDefault(enumerable, searchRequest);
    }

    /// <summary>
    /// Get query with applied filters
    /// </summary>
    /// <returns>Built query</returns>
    public IEnumerable<T> AsEnumerable() => enumerable;
}