using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Instruments.DynamicQueries.Models;
using System;
using Instruments.DynamicQueries.ExpressionBuilders;
using Instruments.DynamicQueries.Reflection;

namespace Instruments.DynamicQueries
{
    public static class QueryExtensions
    {
        public static IQueryable<TEntity> DynamicOrderBy<TEntity>(this IQueryable<TEntity> source, OrderModel orderModel)
        {
            var builder = new OrderExpressionBuilder();

            var orderedSource = builder.ApplyToCollection(source, orderModel);

            return orderedSource;
        }

        public static IQueryable<TEntity> DynamicFilterBy<TEntity>(this IQueryable<TEntity> source, FilterModel orderModel)
        {
            var builder = new FilterExpressionBuilder();

            var orderedSource = builder.ApplyToCollection(source, orderModel);

            return orderedSource;
        }

        public static IQueryable<TEntity> DynamicSearchBy<TEntity>(this IQueryable<TEntity> source, SearchModel searchModel)
        {
            var builder = new SearchExpressionBuilder();

            var orderedSource = builder.ApplyToCollection(source, searchModel);

            return orderedSource;
        }
    }
}
