namespace MKIL.DotnetTest.OrderService.Domain.DTO
{
    public class OrderDto
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } = decimal.Zero;
    }
}
