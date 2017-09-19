using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Instruments.DynamicQueries.Reflection
{
    public class ReflectionHelper
    {
        public static bool IsCollection(Type propertyType)
        {
            if (propertyType == typeof(string))
                return false;

            return propertyType.GetInterfaces().Any(s =>
                s.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                s.GetGenericTypeDefinition() == typeof(ICollection<>));
        }
    }
}
