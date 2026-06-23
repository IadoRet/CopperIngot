using CopperIngot.Interfaces;

namespace CopperIngot.Engine;

/// <summary>
/// Search session for chaining filtering.
/// </summary>
/// <param name="enumerable">Enumerable to filter</param>
/// <param name="searchEngine">Search engine instance</param>
/// <typeparam name="T">Type of the collection</typeparam>
public class EnumerableSearchSession<T>(IEnumerable<T> enumerable, SearchEngine searchEngine)
{
    private IEnumerable<T> _enumerable = enumerable;

    /// <summary>
    /// Default .Where() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>This instance. Use .AsEnumerable() to get a filtered query</returns>
    public EnumerableSearchSession<T> Where(ISearchRequest searchRequest)
    {
        _enumerable = searchEngine.Where(_enumerable, searchRequest);
        return this;
    }

    /// <summary>
    /// Default .Select() behavior
    /// </summary>
    /// <param name="func">Selector function</param>
    /// <returns>A new session instance</returns>
    public EnumerableSearchSession<TNew> Select<TNew>(Func<T, TNew> func)
    {
        return searchEngine.From(_enumerable.Select(func));
    }

    /// <summary>
    /// Default .FirstOrDefault() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>First match or null</returns>
    public T? FirstOrDefault(ISearchRequest searchRequest)
    {
        return searchEngine.FirstOrDefault(_enumerable, searchRequest);
    }

    /// <summary>
    /// Get query with applied filters
    /// </summary>
    /// <returns>Built query</returns>
    public IEnumerable<T> AsEnumerable() => _enumerable;
}