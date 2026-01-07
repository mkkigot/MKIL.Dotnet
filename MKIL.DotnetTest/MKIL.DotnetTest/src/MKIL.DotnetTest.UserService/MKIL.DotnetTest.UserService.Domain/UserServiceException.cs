using MKIL.DotnetTest.Shared.Lib.DTO;
using static MKIL.DotnetTest.Shared.Lib.Utilities.Constants;

namespace MKIL.DotnetTest.UserService.Domain
{
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
