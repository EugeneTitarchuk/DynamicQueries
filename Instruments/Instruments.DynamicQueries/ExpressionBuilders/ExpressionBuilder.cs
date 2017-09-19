using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Instruments.DynamicQueries.Models;
using Instruments.DynamicQueries.Models.Internal;
using Instruments.DynamicQueries.Reflection;

namespace Instruments.DynamicQueries.ExpressionBuilders
{
    public abstract class ExpressionBuilder
    {
        protected ParsedPropertyPath ParsePath(string path, Type entityType)
        {
            var propertyParser = new PropertyParser();
            var propertyPath = propertyParser.ParsePropertyOfType(path, entityType);
            return propertyPath;
        }

        protected Queue<ExpressionPart> CreateExpressionsQueue(ParsedPropertyPath propertyPath, Type collectionEntityType)
        {
            //fill initial list
            var elements = new Queue<ParsedPropertyElement>();
            foreach (var element in propertyPath.Elements)
            {
                elements.Enqueue(element);
            }
           
            //create resulting list
            var expressionParts = new Queue<ExpressionPart>();

            var lastCollectionEntityType = collectionEntityType;

            var index = 0;
            while (elements.Count > 0)
            {
                var expressionPart = CreateExpressionPart(lastCollectionEntityType, elements, index);

                if (expressionPart.CollectionElement != null)
                    lastCollectionEntityType = GetCollectionElementType(expressionPart.CollectionElement);

                expressionParts.Enqueue(expressionPart);
                index++;
            }

            return expressionParts;
        }
        
        protected MethodInfo GetMethodInfo(ParsedPropertyElement collectionElement, KnownFunctions function)
        {
            var method = collectionElement.PropertyInfo.PropertyType.GetMethod(function.ToString());
            return method;
        }

        protected Type GetCollectionElementType(ParsedPropertyElement collectionElement)
        {
            var types = collectionElement.PropertyInfo.PropertyType.GetGenericArguments();

            if (types.Length == 1)
                return types[0];

            throw new DynamicQueryException($"Unsupported collection type {collectionElement.PropertyInfo.Name} of collection {collectionElement.PropertyName}");
        }

        protected ExpressionPart CreateExpressionPart(Type lastCollectionElementType, Queue<ParsedPropertyElement> elements, int index)
        {
            var collectionLambdaParameter = Expression.Parameter(lastCollectionElementType, "p" + index);

            ParsedPropertyElement element;
            MemberExpression memberAccess = null;

            ParsedPropertyElement collectionElement = null;
            
            while (true)
            {
                element = elements.Dequeue();

                if (element.PropertyType == PropertyType.Collection)
                    collectionElement = element;
                
                if (element.PropertyType == PropertyType.CollectionFunction)
                    break;

                memberAccess = Expression.MakeMemberAccess(collectionLambdaParameter, element.PropertyInfo);

                if (elements.Count == 0)
                    break;

                var nextItem = elements.Peek();
                if (element.PropertyType == PropertyType.Collection && nextItem.PropertyType != PropertyType.CollectionFunction)
                    break;
            }

            return new ExpressionPart
            {
                CollectionElement = collectionElement,
                LastPartElement = element,
                MemberAccess = memberAccess,
                Parameter = collectionLambdaParameter
            };
        }






    }
}
