
using MKIL.DotnetTest.UserService.Domain.DTO;

namespace MKIL.DotnetTest.UserService.Domain
{
    public enum StatusCode 
    {
        NotFound = 404,
        ValidationError = 400,
        InternalError = 500 
    }

    public class UserServiceException : Exception
    {
        public UserServiceException(StatusCode statusCode, string msg) : base(msg)
        {
            ErrorCode = statusCode;
        }

        public UserServiceException(StatusCode statusCode, List<ErrorDto> errorList) : base()
        {
            ErrorCode = statusCode;
            ErrorList = errorList;
        }

        public StatusCode ErrorCode { get; set; }
        public List<ErrorDto> ErrorList { get; set; } = new List<ErrorDto>();
    }
}
