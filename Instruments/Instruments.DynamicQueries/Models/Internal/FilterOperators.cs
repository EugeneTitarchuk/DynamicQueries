using System;
using System.Collections.Generic;
using System.Linq;

namespace Instruments.DynamicQueries.Models.Internal
{
    public static class FilterOperators
    {
        private static readonly List<FilterOperator> All;

        private static FilterOperator Equality = new FilterOperator(FilterOperation.Equality, "==", "op_Equality");
        private static FilterOperator Inequality = new FilterOperator(FilterOperation.Inequality, "!=", "op_Inequality");
        private static FilterOperator GreaterThan = new FilterOperator(FilterOperation.GreaterThan, ">", "op_GreaterThan");
        private static FilterOperator LessThan = new FilterOperator(FilterOperation.LessThan, "<", "op_LessThan");
        private static FilterOperator GreaterThanOrEqual = new FilterOperator(FilterOperation.GreaterThanOrEqual, ">=", "op_GreaterThanOrEqual");
        private static FilterOperator LessThanOrEqual = new FilterOperator(FilterOperation.LessThanOrEqual, "<=", "op_LessThanOrEqual");

        static FilterOperators()
        {
            All = new List<FilterOperator>
            {
                Equality,
                Inequality,
                GreaterThan,
                LessThan,
                GreaterThanOrEqual,
                LessThanOrEqual
            };
        }

        public static FilterOperation ParseOperation(string operation)
        {
            if (string.IsNullOrEmpty(operation))
                return FilterOperation.Equality;

            var filterOperationModel = All.FirstOrDefault(s => s.Display == operation);
            if (filterOperationModel != null)
                return filterOperationModel.Type;
            var isSuccess = Enum.TryParse(operation, true, out FilterOperation filterOperation);
            if (isSuccess)
                return filterOperation;
            
            throw new DynamicQueryException($"Can not parse follow operation: {operation}");
        }
    }

    class FilterOperator
    {
        public FilterOperation Type { get; set; }

        public string Display { get; set; }

        public string MethodName { get; set; }

        public FilterOperator(FilterOperation type, string display, string methodName)
        {
            Type = type;
            Display = display;
            MethodName = methodName;
        }
    }
}
