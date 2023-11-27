using CopperIngot.DefaultExpressionBuilders;
using CopperIngot.Interfaces;

namespace CopperIngot.SearchEngine;

/// <summary>
/// Search engine configuration
/// </summary>
public class SearchEngineConfiguration
{
    private readonly Dictionary<Type, IFilterBuilderBase> _expressionBuilders = [];
    private readonly Dictionary<string, SearchAlias> _aliases = [];
    private bool _useCache = true;
    private bool _useAliases;
    
    /// <summary>
    /// Add custom builder
    /// </summary>
    /// <param name="filterBuilder">Custom builder</param>
    /// <returns>This instance</returns>
    public SearchEngineConfiguration WithFilterBuilder<T>(IFilterBuilder<T> filterBuilder)
    {
        _expressionBuilders.Add(typeof(T), filterBuilder);
        return this;
    }

    /// <summary>
    /// Use all explicitly supported types for comparison
    /// </summary>
    /// <returns>This instance</returns>
    public SearchEngineConfiguration WithDefaultBuilders()
    {
        _expressionBuilders.Add(typeof(Guid), new GuidFilterBuilder());
        _expressionBuilders.Add(typeof(DateTime), new DateTimeFilterBuilder());
        _expressionBuilders.Add(typeof(DateOnly), new DateOnlyFilterBuilder());

        return this;
    }

    /// <summary>
    /// Enable or disable filter caching. Enabled by default.
    /// Caching for IQueryable is not supported
    /// </summary>
    /// <returns>This instance</returns>
    public SearchEngineConfiguration UseCache(bool useCache = true)
    {
        _useCache = useCache;
        return this;
    }

    /// <summary>
    /// Enable or disable usage of custom aliases. Disabled by default
    /// </summary>
    /// <returns>This instance</returns>
    public SearchEngineConfiguration UseAliases(bool useAliases = true)
    {
        _useAliases = useAliases;
        return this;
    }

    /// <summary>
    /// Add custom alias
    /// </summary>
    /// <param name="searchAlias">Alias</param>
    /// <returns>This instance</returns>
    public SearchEngineConfiguration WithAlias(SearchAlias searchAlias)
    {
        _aliases.Add(searchAlias.GetName(), searchAlias);
        return this;
    }
    
    internal IDictionary<Type, IFilterBuilderBase> GetCustomBuilders() => _expressionBuilders;

    internal IDictionary<string, SearchAlias> GetAliases() => _aliases;
    
    internal bool ShouldUseCache() => _useCache;

    internal bool ShouldUseAliases() => _useAliases;
}