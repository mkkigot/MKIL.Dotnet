
namespace MKIL.DotnetTest.UserService.Domain.Entities
{
    public class UserOrder
    {
        public int UId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime SyncedAt { get; set; }
    }
}
