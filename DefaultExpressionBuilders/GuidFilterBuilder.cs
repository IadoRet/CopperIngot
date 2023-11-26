using System.Linq.Expressions;
using CopperIngot.Enums;
using CopperIngot.Interfaces;
using CopperIngot.Requests;

namespace CopperIngot.DefaultExpressionBuilders;

public class GuidFilterBuilder : IFilterBuilder<Guid>
{
    public Expression BuildExpressionTree(Expression dataExpression, Expression valueExpression, ISearchRequest searchRequest)
    {
        return searchRequest.Comparison switch
        {
            SearchComparison.Equals => Expression.Equal(dataExpression, valueExpression),
            SearchComparison.NotEquals => Expression.NotEqual(dataExpression, valueExpression),
            SearchComparison.Greater => Expression.GreaterThan(dataExpression, valueExpression),
            SearchComparison.GreaterOrEqual => Expression.GreaterThanOrEqual(dataExpression, valueExpression),
            SearchComparison.Less => Expression.LessThan(dataExpression, valueExpression),
            SearchComparison.LessOrEqual => Expression.LessThanOrEqual(dataExpression, valueExpression),
            _ => throw new ArgumentOutOfRangeException(nameof(searchRequest.Comparison))
        };
    }

    public Guid ConvertValue(StringSearchRequest searchRequest)
    {
        return Guid.Parse((string)searchRequest.GetValue());
    }

    public Guid ConvertValue(ObjectSearchRequest searchRequest)
    {
        return (Guid)searchRequest.GetValue();
    }

    public virtual Guid CustomConvertValue(ISearchRequest searchRequest)
    {
        throw new NotImplementedException();
    }
}