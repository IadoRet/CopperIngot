using CopperIngot.Engine;

namespace CopperIngot.Interfaces;

/// <summary>
/// Search engine. If you are using filter caching use as a singleton.
/// </summary>
public interface ISearchEngine
{
    /// <summary>
    /// Start a new search session for IEnumerable
    /// </summary>
    EnumerableSearchSession<T> From<T>(IEnumerable<T> enumerable);

    /// <summary>
    /// Start a new search session for IQueryable
    /// </summary>
    QueryableSearchSession<T> From<T>(IQueryable<T> query);

    /// <summary>
    /// Configure search engine parameters
    /// </summary>
    /// <param name="configuration">Configuration</param>
    void Configure(SearchEngineConfiguration configuration);
}