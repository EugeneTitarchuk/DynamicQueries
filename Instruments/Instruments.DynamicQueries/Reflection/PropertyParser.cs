using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Instruments.DynamicQueries.Attributes;
using Instruments.DynamicQueries.Cache;
using Instruments.DynamicQueries.Models.Internal;

namespace Instruments.DynamicQueries.Reflection
{
    class PropertyParser
    {
        public ParsedPropertyPath ParsePropertyOfType(string propertyPath, Type entityType)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
                throw new DynamicQueryException("Path to property can not be null or empty");

            var normalizedPath = propertyPath.ToLower();

            var parsedPropertyPath = new ParsedPropertyPath(normalizedPath);
            var pathElements = normalizedPath.Split('.');
            parsedPropertyPath.Elements = new List<ParsedPropertyElement>(pathElements.Length);

            var currentEntityType = entityType;

            ParsedPropertyElement lastElement = null;
            var applyToInternalObject = false;

            foreach (var pathElement in pathElements)
            {
                var isLastElementWasFunction = lastElement?.PropertyType == PropertyType.CollectionFunction;
                if (isLastElementWasFunction)
                    throw new DynamicQueryException($"Function '{lastElement.PropertyName}' should be last element in the property path");
                
                var propertyName = pathElement;
                var propertyType = PropertyType.Complex;
                var functionType = KnownFunctions.None;
                PropertyInfo propertyInfo = null;

                if (pathElement.Contains("[]"))
                {
                    propertyType = PropertyType.Collection;
                    propertyName = pathElement.Substring(0, pathElement.Length - 2);
                    applyToInternalObject = true;
                }
                
                var isLastElementWasCollection = lastElement?.PropertyType == PropertyType.Collection;
                if (isLastElementWasCollection)
                {
                    propertyType = applyToInternalObject ? PropertyType.Complex : PropertyType.CollectionFunction;
                    applyToInternalObject = false;
                    if (propertyType == PropertyType.CollectionFunction)
                    {
                        functionType = ParseFunction(propertyName);
                    }
                }

                if (isLastElementWasCollection && propertyType != PropertyType.CollectionFunction)
                {
                    currentEntityType = lastElement.PropertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
                    if (currentEntityType == null)
                        throw new DynamicQueryException($"Cannot get type of objects in collection '{lastElement.PropertyName}'");
                }
                
                if (propertyType != PropertyType.CollectionFunction)
                {
                    propertyInfo = GetPropertyInfo(propertyName, currentEntityType);

                    if (ReflectionHelper.IsCollection(propertyInfo.PropertyType))
                        propertyType = PropertyType.Collection;

                    currentEntityType = propertyInfo.PropertyType;
                }
                

                lastElement = new ParsedPropertyElement
                {
                    PropertyName = propertyName,
                    PropertyType = propertyType,
                    PropertyInfo = propertyInfo,
                    RawElementName = pathElement,
                    Function = functionType
                };
                
                parsedPropertyPath.Elements.Add(lastElement);
            }
            
            if (lastElement?.PropertyType == PropertyType.Collection)
                throw new DynamicQueryException("Array or Collection cannot be the last element in ");

            if (lastElement?.PropertyType == PropertyType.Complex)
                lastElement.PropertyType = PropertyType.Simple;

            return parsedPropertyPath;
        }

        private PropertyInfo GetPropertyInfo(string propertyName, Type entityType)
        {
            var propertyInfo = ReflectionCache.Instance.GetPropertyInfo(propertyName, entityType);
            if (propertyInfo != null)
                return propertyInfo;

            var properties = entityType.GetProperties();
            propertyInfo = properties.FirstOrDefault(s => s.Name.ToLower() == propertyName);

            if (propertyInfo == null)
            {
                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes(typeof(AlternativeNameAttribute));
                    foreach (var attribute in attributes)
                    {
                        if (attribute is AlternativeNameAttribute alternativeName)
                        {
                            if (alternativeName.NormilizedName == propertyName)
                                return property;
                        }
                    }
                }

                throw new DynamicQueryException($"Can not find specified property of path: {propertyName}");
            }

            ReflectionCache.Instance.SetPropertyInfo(propertyName, entityType, propertyInfo);
            return propertyInfo;
        }

        private KnownFunctions ParseFunction(string functionName)
        {
            if (functionName.ToLower() == "count" || functionName.ToLower() == "length")
                return KnownFunctions.Count;
            if (functionName.ToLower() == "any")
                return KnownFunctions.Any;

            throw new DynamicQueryException($"Unsupported function name: {functionName}");
        }
    }
}
