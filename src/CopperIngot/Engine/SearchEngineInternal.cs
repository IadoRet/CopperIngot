using System.Linq.Expressions;
using System.Reflection;
using CopperIngot.Enums;
using CopperIngot.Exceptions;
using CopperIngot.Interfaces;
using CopperIngot.Requests;

namespace CopperIngot.Engine;

internal static class SearchEngineInternal
{
    internal static Expression<Func<TData, object?, bool>> GetExpression<TData>(
        ISearchRequest searchRequest,
        IDictionary<Type, IFilterBuilderBase> customBuilders,
        PropertyInfo[] propertyInfos)
    {
        return BuildEnumerableExpressionTree<TData>(searchRequest, propertyInfos, customBuilders);
    }

    internal static Expression<Func<TData, bool>> GetQueryableExpression<TData>(
        ISearchRequest searchRequest,
        IDictionary<Type, IFilterBuilderBase> customBuilders,
        PropertyInfo[] propertyInfos,
        object? value)
    {
        return BuildQueryableExpressionTree<TData>(searchRequest, propertyInfos, customBuilders, value);
    }
    
    private static Expression<Func<TData, bool>> BuildQueryableExpressionTree<TData>(
        ISearchRequest request,
        PropertyInfo[] propertyInfos,
        IDictionary<Type, IFilterBuilderBase> customBuilders,
        object? value)
    {
        ParameterExpression dataParameterExpression = Expression.Parameter(typeof(TData));
        Expression comparableExpression = dataParameterExpression;
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            comparableExpression = NullSafePropertyAccessor(comparableExpression, propertyInfos[i]);
        }

        Type comparableType = comparableExpression.Type;
        ConstantExpression valueExpression = Expression.Constant(value, typeof(object));
        Expression convertedParameter = comparableType.IsValueType
            ? Expression.Unbox(valueExpression, comparableType)
            : Expression.Convert(valueExpression, comparableType);

        Expression? comparisonExpression = null;
        
        if (customBuilders.TryGetValue(comparableType, out IFilterBuilderBase? expressionBuilder))
        {
            comparisonExpression = expressionBuilder.BuildExpressionTree(comparableExpression, convertedParameter, request);
        }
        else if (comparableType.IsPrimitive)
        {
            comparisonExpression = BuildPrimitiveExpression(comparableExpression, convertedParameter, request.Comparison);
        }
        else if (comparableType == typeof(string))
        {
            comparisonExpression = BuildStringExpression(comparableExpression, convertedParameter, request, true);
        }

        if (comparisonExpression is null)
            throw new NotImplementedException($"Type {comparableType.Name} is unsupported. You can add custom filter builder if you need to.");
        
        return Expression.Lambda<Func<TData,  bool>>(comparisonExpression, dataParameterExpression);
    }
    
    /// <summary>
    /// Build reusable expression tree for IEnumerable collections
    /// </summary>
    private static Expression<Func<TData, object?, bool>> BuildEnumerableExpressionTree<TData>(
        ISearchRequest request,
        PropertyInfo[] propertyInfos,
        IDictionary<Type, IFilterBuilderBase> customBuilders)
    {
        ParameterExpression dataParameterExpression = Expression.Parameter(typeof(TData));
        Expression comparableExpression = dataParameterExpression;
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            comparableExpression = NullSafePropertyAccessor(comparableExpression, propertyInfos[i]);
        }

        Type comparableType = comparableExpression.Type;
        ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
        Expression convertedParameter = comparableType.IsValueType
            ? Expression.Unbox(parameterExpression, comparableType)
            : Expression.TypeAs(parameterExpression, comparableType);

        Expression? comparisonExpression = null;
        
        if (customBuilders.TryGetValue(comparableType, out IFilterBuilderBase? expressionBuilder))
        {
            comparisonExpression = expressionBuilder.BuildExpressionTree(comparableExpression, convertedParameter, request);
        }
        else if (comparableType.IsPrimitive)
        {
            comparisonExpression = BuildPrimitiveExpression(comparableExpression, convertedParameter, request.Comparison);
        }
        else if (comparableType == typeof(string))
        {
            comparisonExpression = BuildStringExpression(comparableExpression, convertedParameter, request);
        }

        if (comparisonExpression is null)
            throw new NotImplementedException($"Type {comparableType.Name} is unsupported. You can add custom filter builder if you need to.");
        
        return Expression.Lambda<Func<TData, object?, bool>>(comparisonExpression, dataParameterExpression, parameterExpression);
    }

    private static Expression BuildStringExpression(
        Expression comparableExpression,
        Expression parameterExpression,
        ISearchRequest request,
        bool isQueryable = false)
    {
        StringComparison stringComparison = request is StringSearchRequest stringSearchRequest
            ? stringSearchRequest.StringComparison
            : StringComparison.Ordinal;
        
        switch (request.Comparison)
        {
            case SearchComparison.Equals:
            {
                if (isQueryable && IsCaseInsensitive(stringComparison))
                    return BuildQueryableCaseInsensitiveEqualsExpression(comparableExpression, parameterExpression);

                if(stringComparison is StringComparison.Ordinal || isQueryable)
                    return Expression.Equal(comparableExpression, parameterExpression);
                
                MethodInfo stringEqualsMethod = typeof(string).GetMethods()
                    .Single(m => m.Name == nameof(string.Equals) && m.GetParameters().Length == 3);
                    
                ConstantExpression comparisonConstantExpression = Expression.Constant(stringComparison, typeof(StringComparison));
                
                MethodCallExpression equalsCallExpression = Expression.Call(stringEqualsMethod,
                    comparableExpression,
                    parameterExpression,
                    comparisonConstantExpression);
                
                return equalsCallExpression;
            }
            case SearchComparison.NotEquals:            
            {
                if (isQueryable && IsCaseInsensitive(stringComparison))
                    return Expression.Not(BuildQueryableCaseInsensitiveEqualsExpression(comparableExpression, parameterExpression));

                if(stringComparison is StringComparison.Ordinal || isQueryable)
                    return Expression.NotEqual(comparableExpression, parameterExpression);
                
                MethodInfo stringEqualsMethod = typeof(string).GetMethods()
                    .Single(m => m.Name == nameof(string.Equals) && m.GetParameters().Length == 3);

                ConstantExpression comparisonConstantExpression = Expression.Constant(stringComparison, typeof(StringComparison));
                
                MethodCallExpression equalsCallExpression = Expression.Call(stringEqualsMethod,
                    comparableExpression,
                    parameterExpression,
                    comparisonConstantExpression);
                
                return Expression.Not(equalsCallExpression);
            }
            case SearchComparison.Contains:
            case SearchComparison.NotContains:
            {
                Type[] methodParameters = isQueryable
                    ? new[] { typeof(string) }
                    : new[] { typeof(string), typeof(StringComparison) };
                
                MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), methodParameters)!;

                Expression containsExpression;
                if (isQueryable)
                {
                    if (IsCaseInsensitive(stringComparison))
                    {
                        containsExpression = BuildQueryableCaseInsensitiveContainsExpression(comparableExpression, parameterExpression, containsMethod);
                        return request.Comparison is SearchComparison.Contains
                            ? containsExpression
                            : Expression.Not(containsExpression);
                    }

                    containsExpression = Expression.Call(comparableExpression, containsMethod, parameterExpression);
                }
                else
                {
                    ConstantExpression comparisonConstantExpression = Expression.Constant(stringComparison, typeof(StringComparison));
                
                    containsExpression = Expression.Call(comparableExpression, containsMethod, parameterExpression, comparisonConstantExpression);
                }

                return request.Comparison is SearchComparison.Contains
                    ? containsExpression
                    : Expression.Not(containsExpression);
            }
            case SearchComparison.Greater:
            case SearchComparison.GreaterOrEqual:
            case SearchComparison.Less:
            case SearchComparison.LessOrEqual:
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Comparison));
        }
    }

    private static Expression BuildQueryableCaseInsensitiveEqualsExpression(
        Expression comparableExpression,
        Expression parameterExpression)
    {
        BinaryExpression comparableIsNull = Expression.Equal(comparableExpression, Expression.Constant(null, typeof(string)));
        BinaryExpression parameterIsNull = Expression.Equal(parameterExpression, Expression.Constant(null, typeof(string)));
        BinaryExpression comparableIsNotNull = Expression.NotEqual(comparableExpression, Expression.Constant(null, typeof(string)));
        BinaryExpression parameterIsNotNull = Expression.NotEqual(parameterExpression, Expression.Constant(null, typeof(string)));

        return Expression.OrElse(
            Expression.AndAlso(comparableIsNull, parameterIsNull),
            Expression.AndAlso(
                Expression.AndAlso(comparableIsNotNull, parameterIsNotNull),
                Expression.Equal(ToLower(comparableExpression), ToLower(parameterExpression))));
    }

    private static Expression BuildQueryableCaseInsensitiveContainsExpression(
        Expression comparableExpression,
        Expression parameterExpression,
        MethodInfo containsMethod)
    {
        BinaryExpression comparableIsNotNull = Expression.NotEqual(comparableExpression, Expression.Constant(null, typeof(string)));
        BinaryExpression parameterIsNotNull = Expression.NotEqual(parameterExpression, Expression.Constant(null, typeof(string)));

        return Expression.AndAlso(
            Expression.AndAlso(comparableIsNotNull, parameterIsNotNull),
            Expression.Call(ToLower(comparableExpression), containsMethod, ToLower(parameterExpression)));
    }

    private static MethodCallExpression ToLower(Expression expression)
    {
        MethodInfo toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        return Expression.Call(expression, toLowerMethod);
    }

    private static bool IsCaseInsensitive(StringComparison stringComparison)
    {
        return stringComparison is StringComparison.CurrentCultureIgnoreCase
            or StringComparison.InvariantCultureIgnoreCase
            or StringComparison.OrdinalIgnoreCase;
    }

    private static BinaryExpression BuildPrimitiveExpression(
        Expression comparableExpression,
        Expression parameterExpression,
        SearchComparison comparison)
    {
        return comparison switch
        {
            SearchComparison.Equals => Expression.Equal(comparableExpression, parameterExpression),
            SearchComparison.NotEquals => Expression.NotEqual(comparableExpression, parameterExpression),
            SearchComparison.Greater => Expression.GreaterThan(comparableExpression, parameterExpression),
            SearchComparison.GreaterOrEqual => Expression.GreaterThanOrEqual(comparableExpression, parameterExpression),
            SearchComparison.Less => Expression.LessThan(comparableExpression, parameterExpression),
            SearchComparison.LessOrEqual => Expression.LessThanOrEqual(comparableExpression, parameterExpression),
            _ => throw new ArgumentOutOfRangeException(nameof(comparison))
        };
    }

    private static Expression NullSafePropertyAccessor(Expression expression, PropertyInfo propertyInfo)
    {
        bool expressionIsValueType = expression.Type.IsValueType;
        if (expressionIsValueType)
            return Expression.Property(expression, propertyInfo);
        
        BinaryExpression notNullExpression = Expression.NotEqual(expression, Expression.Constant(null, expression.Type));
        bool propertyIsValueType = propertyInfo.PropertyType.IsValueType;
        
        return Expression.Condition(notNullExpression,
                   Expression.Property(expression, propertyInfo),
                   propertyIsValueType 
                       ? Expression.Default(propertyInfo.PropertyType) 
                       : Expression.Constant(null, propertyInfo.PropertyType));
    }

    internal static PropertyInfo[] GetPropertyTree<T>(string property)
    {
        string[] propertyTree = property.Split('.');
        Type dataType = typeof(T);

        PropertyInfo[] propertyInfos = new PropertyInfo[propertyTree.Length];
        
        for (int i = 0; i < propertyTree.Length; i++)
        {
            PropertyInfo? propertyInfo = dataType.GetProperty(propertyTree[i]);
            propertyInfos[i] = propertyInfo ?? throw new PropertyNotFoundException(propertyTree[i], typeof(T));
            dataType = propertyInfo.PropertyType;
        }

        return propertyInfos;
    }
}
