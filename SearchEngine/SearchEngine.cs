using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using CopperIngot.Interfaces;
using CopperIngot.Requests;

namespace CopperIngot.SearchEngine;

public class SearchEngine : ISearchEngine
{
    private readonly ConcurrentDictionary<int, ICachedFilter> _cachedFilters = new();
    private FrozenDictionary<string, SearchAlias> _aliases;
    private FrozenDictionary<Type, IFilterBuilderBase> _customFilterBuilders;
    private bool _useCache;
    private bool _useAliases;

    public SearchEngine(SearchEngineConfiguration configuration)
    {
        _aliases = configuration.GetAliases().ToFrozenDictionary();
        _customFilterBuilders = configuration.GetCustomBuilders().ToFrozenDictionary();
        _useCache = configuration.ShouldUseCache();
        _useAliases = configuration.ShouldUseAliases();
    }

    public SearchEngine()
    {
        _aliases = FrozenDictionary<string, SearchAlias>.Empty;
        _customFilterBuilders = FrozenDictionary<Type, IFilterBuilderBase>.Empty;
        _useCache = true;
        _useAliases = false;
    }

    public void Configure(SearchEngineConfiguration configuration)
    {
        _aliases = configuration.GetAliases().ToFrozenDictionary();
        _customFilterBuilders = configuration.GetCustomBuilders().ToFrozenDictionary();
        _useCache = configuration.ShouldUseCache();
        _useAliases = configuration.ShouldUseAliases();
    }
    
    public EnumerableSearchSession<T> From<T>(IEnumerable<T> enumerable)
    {
        return new EnumerableSearchSession<T>(enumerable, this);
    }

    public QueryableSearchSession<T> From<T>(IQueryable<T> query)
    {
        return new QueryableSearchSession<T>(query, this);
    }
    
    internal IEnumerable<T> Where<T>(IEnumerable<T> values, ISearchRequest searchRequest)
    {
        CachedFilter<T> cachedFilter = GetAndCacheFilter<T>(searchRequest);
        object? filterValue = Convert(searchRequest, cachedFilter.ComparableType);
        
        return cachedFilter.Where(values, filterValue);
    }

    internal T? FirstOrDefault<T>(IEnumerable<T> values, ISearchRequest searchRequest)
    {
        CachedFilter<T> cachedFilter = GetAndCacheFilter<T>(searchRequest);
        object? filterValue = Convert(searchRequest, cachedFilter.ComparableType);
        
        return cachedFilter.FirstOrDefault(values, filterValue);
    }

    internal IQueryable<T> Where<T>(IQueryable<T> values, ISearchRequest searchRequest)
    {
        Expression<Func<T, bool>> expression = GetQueryableExpression<T>(searchRequest);

        return values.Where(expression);
    }

    internal T? FirstOrDefault<T>(IQueryable<T> values, ISearchRequest searchRequest)
    {
        Expression<Func<T, bool>> expression = GetQueryableExpression<T>(searchRequest);

        return values.FirstOrDefault(expression);
    }

    private CachedFilter<T> GetAndCacheFilter<T>(ISearchRequest searchRequest)
    {
        int key = searchRequest.GetUniqueKey();
        if (_useCache && _cachedFilters.TryGetValue(key, out ICachedFilter? storedFilter)
                      && storedFilter is CachedFilter<T> cachedFilter)
        {
            return cachedFilter;
        }
                
        PropertyInfo[]? properties = null;
        if (_useAliases && _aliases.TryGetValue(searchRequest.Property, out SearchAlias? alias))
            properties = alias.GetProperties();
        
        properties ??= SearchEngineInternal.GetPropertyTree<T>(searchRequest.Property);
        
        Func<T, object?, bool> filter = SearchEngineInternal.GetExpression<T>(searchRequest, _customFilterBuilders, properties)
                                                            .Compile();
        
        cachedFilter = new CachedFilter<T>(filter, properties.Last().PropertyType);
        if(_useCache)
            _cachedFilters.TryAdd(key, cachedFilter);

        return cachedFilter;
    }
    
    private Expression<Func<T, bool>> GetQueryableExpression<T>(ISearchRequest searchRequest)
    {
        PropertyInfo[]? aliasProperties = null;
        if (_useAliases && _aliases.TryGetValue(searchRequest.Property, out SearchAlias? alias))
            aliasProperties = alias.GetProperties();

        aliasProperties ??= SearchEngineInternal.GetPropertyTree<T>(searchRequest.Property);

        Type comparableType = aliasProperties.Last().PropertyType;
        object? filterValue = Convert(searchRequest, comparableType);
        
        Expression<Func<T, bool>> expression = SearchEngineInternal.GetQueryableExpression<T>(searchRequest, _customFilterBuilders, aliasProperties, filterValue);

        return expression;
    }


    private object? Convert(ISearchRequest searchRequest, Type comparableType)
    {
        object value = searchRequest.GetValue();
        if (_customFilterBuilders.TryGetValue(comparableType, out IFilterBuilderBase? expressionBuilder))
            return expressionBuilder.ConvertValue(searchRequest);
        
        if (!comparableType.IsPrimitive) 
            return searchRequest.GetValue();
        
        switch (searchRequest)
        {
            //Parse if value comes as a string.
            case StringSearchRequest stringSearchRequest:
                TypeConverter converter = new();
                string stringValue = (string)searchRequest.GetValue();
                return converter.ConvertTo(stringValue, comparableType);
            default:
                return value;
        }
    }
}