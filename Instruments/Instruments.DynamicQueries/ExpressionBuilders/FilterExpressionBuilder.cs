using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Instruments.DynamicQueries.Models;
using Instruments.DynamicQueries.Models.Internal;

namespace Instruments.DynamicQueries.ExpressionBuilders
{
    public class FilterExpressionBuilder : ExpressionBuilder
    {
        public FilterExpressionBuilder()
        {

        }

        public IQueryable<TEntity> ApplyToCollection<TEntity>(IQueryable<TEntity> collection, FilterModel filterModel)
        {
            var rootType = typeof(TEntity);

            Expression summaryExpression = null;

            foreach (var orderItem in filterModel.FilterItems)
            {
                var parsedPath = ParsePath(orderItem.Field, rootType);
                var partsQueue = CreateExpressionsQueue(parsedPath, rootType);

                var operation = FilterOperators.ParseOperation(orderItem.Operation);

                var lambda = BuildLambdaExpression(partsQueue, orderItem.Value, operation);

                summaryExpression = summaryExpression == null
                    ? (Expression) lambda
                    : Expression.AndAlso(summaryExpression, lambda);
            }

            var expressionArgs = new[] { rootType };

            var unaryExpression = Expression.Quote(summaryExpression);
            var resultExpression = Expression.Call(typeof(Queryable), "Where", expressionArgs.ToArray(), collection.Expression, unaryExpression);

            collection = collection.Provider.CreateQuery<TEntity>(resultExpression);

            return collection;
        }

        private LambdaExpression BuildLambdaExpression(Queue<ExpressionPart> restExpressionParts, string value, FilterOperation operationType)
        {
            var currentExpressionPart = restExpressionParts.Dequeue();

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.CollectionFunction)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);
                var genericTypeArgs = new[] { collectionElementsType };

                var functionName = currentExpressionPart.LastPartElement.Function.ToString();

                var functionExpression = Expression.Call(typeof(Enumerable), functionName, genericTypeArgs, currentExpressionPart.MemberAccess);

                var comparingExpression = BuildComparationExpression(value, functionExpression, operationType);

                return Expression.Lambda(comparingExpression, currentExpressionPart.Parameter);
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Collection)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);

                var lambdaExpressionForNestedElements = BuildLambdaExpression(restExpressionParts, value, operationType);

                var genericTypeArgs = new[] { collectionElementsType };

                var joinedExpression = Expression.Call(typeof(Enumerable), "All", genericTypeArgs, currentExpressionPart.MemberAccess, lambdaExpressionForNestedElements);
                
                return Expression.Lambda(joinedExpression, currentExpressionPart.Parameter);
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Simple)
            {
                var comparingExpression = BuildComparationExpression(value, currentExpressionPart.MemberAccess, operationType);

                return Expression.Lambda(comparingExpression, currentExpressionPart.Parameter);
            }

            return null;
        }

        private Expression BuildComparationExpression(string constValue, Expression variableToCompare, FilterOperation compareOperationType)
        {
            var variableToCompareType = variableToCompare.Type;
            
            TypeConverter convertToMember = TypeDescriptor.GetConverter(variableToCompareType);
            var convertedValue = convertToMember.ConvertFrom(constValue);
            var paramExpression = Expression.Constant(convertedValue);
            var parameter = Expression.Convert(paramExpression, variableToCompareType);

            switch (compareOperationType)
            {
                case FilterOperation.Equality:
                    return Expression.Equal(variableToCompare, parameter);
                case FilterOperation.Inequality:
                    return Expression.NotEqual(variableToCompare, parameter);
                case FilterOperation.GreaterThan:
                    return Expression.GreaterThan(variableToCompare, parameter);
                case FilterOperation.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(variableToCompare, parameter);
                case FilterOperation.LessThan:
                    return Expression.LessThan(variableToCompare, parameter);
                case FilterOperation.LessThanOrEqual:
                    return Expression.LessThanOrEqual(variableToCompare, parameter);
                default:
                    throw new DynamicQueryException("Unsupported comparing operation");
            }
        }
    }
}
