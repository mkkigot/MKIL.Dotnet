namespace MKIL.DotnetTest.Shared.Lib.Utilities
{
    public static class Constants
    {
        public const string CORRELATION_HEADER = "X-Correlation-ID";

        public enum StatusCode
        {
            NotFound = 404,
            ValidationError = 400,
            InternalError = 500
        }
    }
}
