namespace Instruments.DynamicQueries.Models
{
    public class OrderModel
    {
        public OrderItem[] OrderItems { get; set; }
    }

    public class OrderItem
    {
        public string Field { get; set; }
        public string Direction { get; set; }
    }

    public enum OrderDirection
    {
        Ascending,
        Descending
    }

    public static class OrderDirectionHelper
    {
        public static OrderDirection Parse(string direction)
        {
            if (direction.ToLower().StartsWith("asc"))
                return OrderDirection.Ascending;
            return OrderDirection.Descending;
        }
    }
}
