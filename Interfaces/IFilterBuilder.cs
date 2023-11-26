using System.Linq.Expressions;
using CopperIngot.Requests;

namespace CopperIngot.Interfaces;

/// <summary>
/// Builder for processing custom or unsupported types.
/// </summary>
/// <typeparam name="T">Comparable type</typeparam>
public interface IFilterBuilder<out T> : IFilterBuilderBase
{
    /// <summary>
    /// Convert value from StringRequest (Parse, etc.)
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>Converted value</returns>
    T ConvertValue(StringSearchRequest searchRequest);

    /// <summary>
    /// Convert value from ObjectRequest (for specific cases only)
    /// </summary>
    /// <param name="searchRequest"></param>
    /// <returns>Converted value</returns>
    T ConvertValue(ObjectSearchRequest searchRequest);

    /// <summary>
    /// Converter for processing custom type requests
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>Converted value</returns>
    T CustomConvertValue(ISearchRequest searchRequest);

    object? IFilterBuilderBase.ConvertValue(ISearchRequest searchRequest)
    {        
        return searchRequest switch
        {
            StringSearchRequest stringSearchRequest => ConvertValue(stringSearchRequest),
            ObjectSearchRequest objectSearchRequest => ConvertValue(objectSearchRequest),
            _ => CustomConvertValue(searchRequest)
        };
    }
}

public interface IFilterBuilderBase
{
    /// <summary>
    /// Convert value from search request
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>Converted value from request</returns>
    object? ConvertValue(ISearchRequest searchRequest);
    
    /// <summary>
    /// Build expression tree for each comparison
    /// </summary>
    /// <param name="dataExpression">Comparable property expression (left)</param>
    /// <param name="valueExpression">Value expression (right)</param>
    /// <param name="searchRequest">Search request</param>
    /// <returns></returns>
    Expression BuildExpressionTree(Expression dataExpression, Expression valueExpression, ISearchRequest searchRequest);
}
