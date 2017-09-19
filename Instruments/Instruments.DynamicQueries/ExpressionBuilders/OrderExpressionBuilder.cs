using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Instruments.DynamicQueries.Models;
using Instruments.DynamicQueries.Models.Internal;

namespace Instruments.DynamicQueries.ExpressionBuilders
{
    public class OrderExpressionBuilder : ExpressionBuilder
    {
        public OrderExpressionBuilder()
        {

        }

        public IQueryable<TEntity> ApplyToCollection<TEntity>(IQueryable<TEntity> collection, OrderModel orderModel)
        {
            var rootType = typeof(TEntity);

            foreach (var orderItem in orderModel.OrderItems)
            {
                var parsedPath = ParsePath(orderItem.Field, rootType);
                var partsQueue = CreateExpressionsQueue(parsedPath, rootType);

                var lambda = BuildLambdaExpression(partsQueue);

                var expressionArgs = new[] { rootType, lambda.Body.Type };
               
                var unaryExpression = Expression.Quote(lambda);

                var orderDirection = OrderDirectionHelper.Parse(orderItem.Direction);

                var methodName = orderDirection == OrderDirection.Ascending ? "OrderBy" : "OrderByDescending";

                var resultExpression = Expression.Call(typeof(Queryable), methodName, expressionArgs.ToArray(), collection.Expression, unaryExpression);

                collection = collection.Provider.CreateQuery<TEntity>(resultExpression);
            }

            return collection;
        }

        private LambdaExpression BuildLambdaExpression(Queue<ExpressionPart> restExpressionParts)
        {
            var currentExpressionPart = restExpressionParts.Dequeue();

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.CollectionFunction)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);
                var functionName = currentExpressionPart.LastPartElement.Function.ToString();

                var genericTypeArgs = new[] {collectionElementsType};

                var functionExpression = Expression.Call(typeof(Enumerable), functionName, genericTypeArgs, currentExpressionPart.MemberAccess);
                
                return Expression.Lambda(functionExpression, currentExpressionPart.Parameter);
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Collection)
            {
                throw new DynamicQueryException($"Not support order for internal collections. Collection: {currentExpressionPart.LastPartElement.RawElementName}");
            }
            
            return Expression.Lambda(currentExpressionPart.MemberAccess, currentExpressionPart.Parameter);
        }
    }
}
