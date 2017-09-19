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

            foreach (var field in filterModel.Fields)
            {
                var parsedPath = ParsePath(field, rootType);
                var partsQueue = CreateExpressionsQueue(parsedPath, rootType);
                
                var lambda = BuildLambdaExpression(partsQueue, filterModel.Value);

                summaryExpression = summaryExpression == null
                    ? (Expression)lambda
                    : Expression.AndAlso(summaryExpression, lambda);
            }

            var expressionArgs = new[] { rootType };

            var unaryExpression = Expression.Quote(summaryExpression);
            var resultExpression = Expression.Call(typeof(Queryable), "Where", expressionArgs.ToArray(), collection.Expression, unaryExpression);

            collection = collection.Provider.CreateQuery<TEntity>(resultExpression);

            return collection;
        }

        private LambdaExpression BuildLambdaExpression(Queue<ExpressionPart> restExpressionParts, string value)
        {
            var currentExpressionPart = restExpressionParts.Dequeue();

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.CollectionFunction)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);
                var genericTypeArgs = new[] { collectionElementsType };

                var functionName = currentExpressionPart.LastPartElement.Function.ToString();

                var functionExpression = Expression.Call(typeof(Enumerable), functionName, genericTypeArgs, currentExpressionPart.MemberAccess);

                var comparingExpression = BuildComparationExpression(value, functionExpression);

                return Expression.Lambda(comparingExpression, currentExpressionPart.Parameter);
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Collection)
            {
                var collectionElementsType = GetCollectionElementType(currentExpressionPart.CollectionElement);

                var lambdaExpressionForNestedElements = BuildLambdaExpression(restExpressionParts, value);

                var genericTypeArgs = new[] { collectionElementsType };

                var joinedExpression = Expression.Call(typeof(Enumerable), "Any", genericTypeArgs, currentExpressionPart.MemberAccess, lambdaExpressionForNestedElements);

                return Expression.Lambda(joinedExpression, currentExpressionPart.Parameter);
            }

            if (currentExpressionPart.LastPartElement.PropertyType == PropertyType.Simple)
            {
                var comparingExpression = BuildComparationExpression(value, currentExpressionPart.MemberAccess);

                return Expression.Lambda(comparingExpression, currentExpressionPart.Parameter);
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
