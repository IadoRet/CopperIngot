using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

/// <summary>
/// Search session for chaining filtering.
/// </summary>
/// <param name="query">Queryable to filter</param>
/// <param name="searchEngine">Search engine instance</param>
/// <typeparam name="T">Type of the collection</typeparam>
public class QueryableSearchSession<T>(IQueryable<T> query, SearchEngine searchEngine)
{
    /// <summary>
    /// Default .Where() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>This instance. Use .AsQueryable() to get filtered query</returns>
    public QueryableSearchSession<T> Where(ISearchRequest searchRequest)
    {
        query = searchEngine.Where(query, searchRequest);
        return this;
    }

    /// <summary>
    /// Default .FirstOrDefault() behavior
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>First match or null</returns>
    public T? FirstOrDefault(ISearchRequest searchRequest)
    {
        return searchEngine.FirstOrDefault(query, searchRequest);
    }

    /// <summary>
    /// Get query with applied filters
    /// </summary>
    /// <returns>Built query</returns>
    public IEnumerable<T> AsQueryable() => query;
}