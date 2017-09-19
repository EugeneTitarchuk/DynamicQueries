namespace Instruments.DynamicQueries.Models
{
    public class FilterModel
    {
        public FilterItem[] FilterItems { get; set; }
    }

    public class FilterItem
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public string Operation { get; set; }
    }

    public enum FilterOperation
    {
        Equality,
        Inequality,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }
}
