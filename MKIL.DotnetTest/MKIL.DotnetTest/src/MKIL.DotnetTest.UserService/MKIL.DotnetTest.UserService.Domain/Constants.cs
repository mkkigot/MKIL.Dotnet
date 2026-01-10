namespace MKIL.DotnetTest.UserService.Domain
{
    public static class Constants
    {
        public const string VALIDATION_ERROR_Email_Required = "Email is required";
        public const string VALIDATION_ERROR_Email_InvalidEmail = "Email must be a valid email address";
        public const string VALIDATION_ERROR_Email_TooLong = "Email is too long";
        public const string VALIDATION_ERROR_Email_Duplicate = "Email already exists";
        public const string VALIDAITON_ERROR_Name_Required = "Name is required";
        public const string VALIDAITON_ERROR_UserId_Required = "UserId is required";
        public const string VALIDAITON_ERROR_UserId_NotExists = "UserId does not exists";
        public const string VALIDAITON_ERROR_OrderId_Required = "OrderId is required";

    }
}
