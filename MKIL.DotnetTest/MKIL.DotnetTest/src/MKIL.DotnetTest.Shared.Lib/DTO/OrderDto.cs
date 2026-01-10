namespace MKIL.DotnetTest.Shared.Lib.DTO
{
    public class OrderDto
    {
        public OrderDto() { }

        public OrderDto(Guid userid, string productName, int qty, decimal price)        
        {
            UserId = userid;
            ProductName = productName;
            Quantity = qty;
            Price = price;
        }

        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } = decimal.Zero;
    }
}
