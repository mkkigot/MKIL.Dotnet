namespace MKIL.DotnetTest.Shared.Lib.DTO
{
    public class UserDto
    {
        public UserDto() { }
        public UserDto(Guid userId, string email)
        {
            Id = userId;
            Email = email;
        }

        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
