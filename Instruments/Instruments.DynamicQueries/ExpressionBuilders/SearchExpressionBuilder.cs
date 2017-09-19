using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Instruments.DynamicQueries.Models;
using Instruments.DynamicQueries.Models.Internal;

namespace Instruments.DynamicQueries.ExpressionBuilders
{
    public class SearchExpressionBuilder : ExpressionBuilder
    {
        public SearchExpressionBuilder()
        {

        }

        public IQueryable<TEntity> ApplyToCollection<TEntity>(IQueryable<TEntity> collection, SearchModel filterModel)
        {
            var rootType = typeof(TEntity);

            Expression summaryExpression = null;

            var rootParam = Expression.Parameter(rootType, "p0");

            foreach (var field in filterModel.Fields)
            {
                var parsedPath = ParsePath(field, rootType);
                var partsQueue = CreateExpressionsQueue(parsedPath, rootType, rootParam);

                var expression = BuildExpressionForField(partsQueue, filterModel.Value);

                summaryExpression = summaryExpression == null
                    ? expression
                    : Expression.OrElse(summaryExpression, expression);
            }

            var expressionArgs = new[] { rootType };

            var lambda = Expression.Lambda(summaryExpression, rootParam);
            var unaryExpression = Expression.Quote(lambda);
            var resultExpression = Expression.Call(typeof(Queryable), "Where", expressionArgs.ToArray(), collection.Expression, unaryExpression);

            collection = collection.Provider.CreateQuery<TEntity>(resultExpression);

            return collection;
        }

        private Expression BuildExpressionForField(Queue<ExpressionPart> restExpressionParts, string value)
        {
            var currentExpressionPart = restExpressionParts.Dequeue();

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.CollectionFunction)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);
                var genericTypeArgs = new[] { collectionElementsType };

                var functionName = currentExpressionPart.LastPartElement.Function.ToString();

                var functionExpression = Expression.Call(typeof(Enumerable), functionName, genericTypeArgs, currentExpressionPart.MemberAccess);

                var comparingExpression = BuildComparationExpression(value, functionExpression);

                return comparingExpression;
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Collection)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);

                var parameterForNestedElements = restExpressionParts.Peek().Parameter;
                var expressionForNestedElements = BuildExpressionForField(restExpressionParts, value);
                var lambdaExpressionForNestedElements = Expression.Lambda(expressionForNestedElements, parameterForNestedElements);

                var genericTypeArgs = new[] { collectionElementsType };

                var joinedExpression = Expression.Call(typeof(Enumerable), "Any", genericTypeArgs, currentExpressionPart.MemberAccess, lambdaExpressionForNestedElements);

                return joinedExpression;
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Simple)
            {
                var comparingExpression = BuildComparationExpression(value, currentExpressionPart.MemberAccess);

                return comparingExpression;
            }

            return null;
        }

        private Expression BuildComparationExpression(string constValue, Expression variableToCompare)
        {
            TypeConverter convertToString = TypeDescriptor.GetConverter(typeof(string));
            var convertedValue = convertToString.ConvertFrom(constValue.ToLower());
            var paramExpression = Expression.Constant(convertedValue);
            var parameter = Expression.Convert(paramExpression, typeof(string));

            var property = Expression.Convert(variableToCompare, typeof(string));

            var toLower = typeof(string).GetMethods().Where(s => s.Name == "ToLower").FirstOrDefault(s => s.GetParameters().Length == 0);
            var normilizedProperty = Expression.Call(property, toLower);

            var contains = typeof(string).GetMethod("Contains");
            return Expression.Call(normilizedProperty, contains, parameter);
        }

    }
}
