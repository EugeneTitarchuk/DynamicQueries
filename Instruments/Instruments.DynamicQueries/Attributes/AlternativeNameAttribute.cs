using System;
using System.Collections.Generic;
using System.Text;

namespace Instruments.DynamicQueries.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AlternativeNameAttribute : Attribute
    {
        public string Name { get; }

        public string NormilizedName { get; }

        public AlternativeNameAttribute(string name)
        {
            Name = name;
            NormilizedName = name.ToLower();
        }
    }
}
