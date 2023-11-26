using CopperIngot.Enums;

namespace CopperIngot.Interfaces;

/// <summary>
/// Search request interface
/// </summary>
public interface ISearchRequest
{
    SearchComparison Comparison { get; }
    
    /// <summary>
    /// Full property path or alias
    /// </summary>
    string Property { get; }

    /// <summary>
    /// Get value for comparison
    /// </summary>
    /// <returns>Value</returns>
    object GetValue();

    /// <summary>
    /// Get unique request identifier for caching.
    /// If caching is disabled return 0
    /// </summary>
    /// <returns>Unique identifier</returns>
    int GetUniqueKey();
}