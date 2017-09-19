using System.Linq.Expressions;

namespace Instruments.DynamicQueries.Models.Internal
{
    public class ExpressionPart
    {
        public ParameterExpression Parameter { get; set; }

        public MemberExpression MemberAccess { get; set; }

        public ParsedPropertyElement CollectionElement { get; set; }

        public ParsedPropertyElement LastPartElement { get; set; }
    }
}
