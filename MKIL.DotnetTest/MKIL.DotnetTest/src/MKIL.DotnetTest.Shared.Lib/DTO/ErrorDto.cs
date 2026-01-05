namespace MKIL.DotnetTest.Shared.Lib.DTO
{
    public class ErrorDto
    {
        public ErrorDto(string field, string message) 
        {
            Field = field;
            Message = message;
        }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
