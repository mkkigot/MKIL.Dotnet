namespace MKIL.DotnetTest.OrderService.Domain.Entities
{
    public class UserCache
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime SyncedAt { get; set; }
    }
}
