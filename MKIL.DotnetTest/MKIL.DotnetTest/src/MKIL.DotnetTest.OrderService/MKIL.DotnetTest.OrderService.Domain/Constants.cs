namespace MKIL.DotnetTest.OrderService.Domain
{
    public static class Constants
    {
        public const string VALIDATION_ERROR_UserId_Required = "UserId is required";
        public const string VALIDATION_ERROR_UserId_NotExists = "User cache does not exists";
        public const string VALIDATION_ERROR_ProductName_Required = "Product Name is required";
        public const string VALIDATION_ERROR_ProductName_MaxLengthExceed = "Product Name max length exceeds";
        public const string VALIDATION_ERROR_Price_Required = "Price is required";
        public const string VALIDATION_ERROR_Quantity_RequiredAndGreaterthan0 = "Quantity should be greater than 0";
    }
}
