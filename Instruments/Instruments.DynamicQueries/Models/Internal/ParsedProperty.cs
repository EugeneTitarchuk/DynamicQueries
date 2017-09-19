using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Instruments.DynamicQueries.Models.Internal
{
    public class ParsedPropertyPath
    {
        public string Path { get; set; }

        public List<ParsedPropertyElement> Elements { get; set; }

        public ParsedPropertyPath(string path)
        {
            Path = path;
        }
    }

    public class ParsedPropertyElement
    {
        public string RawElementName { get; set; }

        public string PropertyName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public PropertyType PropertyType { get; set; }

        public bool NeedInternalExpression { get; set; }

        public KnownFunctions Function { get; set; }
    }

    public enum PropertyType
    {
        Complex,
        Simple, 
        Collection,
        CollectionFunction
    }
}
