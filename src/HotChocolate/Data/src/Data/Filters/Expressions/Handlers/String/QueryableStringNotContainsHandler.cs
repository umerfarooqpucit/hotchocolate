using System.Linq.Expressions;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Data.Filters.Expressions;

public class QueryableStringNotContainsHandler : QueryableStringOperationHandler
{
    public QueryableStringNotContainsHandler(InputParser inputParser) : base(inputParser)
    {
        CanBeNull = false;
    }

    protected override int Operation => DefaultFilterOperations.NotContains;

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object parsedValue)
    {
        Expression property = context.GetInstance();
        return FilterExpressionBuilder.NotContains(property, parsedValue);
    }
}
