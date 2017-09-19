using System;
using System.Collections.Generic;
using System.Text;

namespace Instruments.DynamicQueries.Models.Internal
{
    public class DynamicQueryException : Exception
    {
        public DynamicQueryException(string message) : base(message) { }
    }
}
