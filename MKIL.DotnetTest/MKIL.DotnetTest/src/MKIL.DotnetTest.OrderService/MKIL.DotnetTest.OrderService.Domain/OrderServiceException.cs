using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.OrderService.Domain
{
    public enum StatusCode 
    {
        NotFound = 404,
        ValidationError = 400,
        InternalError = 500 
    }

    public class OrderServiceException : Exception
    {
        public OrderServiceException(StatusCode statusCode, string msg) : base(msg)
        {
            ErrorCode = statusCode;
        }

        public OrderServiceException(StatusCode statusCode, List<ErrorDto> errorList) : base()
        {
            ErrorCode = statusCode;
            ErrorList = errorList;
        }

        public StatusCode ErrorCode { get; set; }
        public List<ErrorDto> ErrorList { get; set; } = new List<ErrorDto>();
    }
}
