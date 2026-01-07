
using Confluent.Kafka;
using System.Data.Common;
using System.Net.Sockets;
using System.Text.Json;

namespace MKIL.DotnetTest.Shared.Lib.Utilities
{
    public static class TransientErrorDetector
    {
        /// <summary>
        /// Determines if an exception represents a transient error that should be retried
        /// </summary>
        public static bool IsTransient(Exception ex)
        {
            return IsKafkaTransient(ex)
                || IsNetworkTransient(ex);
                //|| IsDatabaseTransient(ex); // Commenting as this isn't needed in the test scope
        }

        /// <summary>
        /// Determines if an exception represents a permanent error that should NOT be retried
        /// </summary>
        public static bool IsPermanent(Exception ex)
        {
            // Serialization errors
            if (ex is JsonException || ex is JsonException)
                return true;

            // Validation errors
            if (ex is ArgumentException || ex is ArgumentNullException
                || ex is ArgumentOutOfRangeException)
                return true;

            // Authorization/Authentication
            if (ex is UnauthorizedAccessException)
                return true;

            if (ex.Message.Contains("authorization", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("permission", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
                return true;

            // Not found errors
            if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                return true;

            // Format/parsing errors
            if (ex is FormatException || ex is OverflowException)
                return true;

            // Kafka permanent errors
            if (ex is KafkaException kafkaEx)
            {
                return kafkaEx.Error.Code switch
                {
                    ErrorCode.UnknownTopicOrPart => true,
                    ErrorCode.InvalidMsg => true,
                    ErrorCode.MsgSizeTooLarge => true,
                    ErrorCode.InvalidGroupId => true,
                    ErrorCode.TopicAuthorizationFailed => true,
                    ErrorCode.GroupAuthorizationFailed => true,
                    ErrorCode.InvalidConfig => true,
                    _ => false
                };
            }

            return false;
        }

        private static bool IsKafkaTransient(Exception ex)
        {
            if (ex is not KafkaException kafkaEx)
                return false;

            return kafkaEx.Error.Code switch
            {
                // Network/Connection issues
                ErrorCode.NetworkException => true,
                ErrorCode.RequestTimedOut => true,
                ErrorCode.BrokerNotAvailable => true,
                ErrorCode.NotController => true,
                ErrorCode.LeaderNotAvailable => true,
                ErrorCode.ReplicaNotAvailable => true,

                // Coordinator issues
                ErrorCode.GroupCoordinatorNotAvailable => true,
                ErrorCode.NotCoordinatorForGroup => true,
                //ErrorCode.NotCoordinator => true,

                // Temporary state issues
                ErrorCode.PreferredLeaderNotAvailable => true,
                ErrorCode.GroupLoadInProgress => true,
                ErrorCode.RebalanceInProgress => true,

                // Throttling
                ErrorCode.ThrottlingQuotaExceeded => true,

                _ => false
            };
        }

        private static bool IsNetworkTransient(Exception ex)
        {
            // Socket exceptions
            if (ex is SocketException socketEx)
            {
                return socketEx.SocketErrorCode is
                    SocketError.TimedOut or
                    SocketError.ConnectionRefused or
                    SocketError.ConnectionReset or
                    SocketError.ConnectionAborted or
                    SocketError.HostDown or
                    SocketError.HostUnreachable or
                    SocketError.NetworkDown or
                    SocketError.NetworkUnreachable or
                    SocketError.NetworkReset or
                    SocketError.TryAgain or
                    SocketError.WouldBlock;
            }

            // HTTP exceptions
            if (ex is HttpRequestException httpEx)
            {
                // Check inner socket exception
                if (httpEx.InnerException is SocketException innerSocket)
                    return IsNetworkTransient(innerSocket);

                // Check message for timeout/connection
                var message = httpEx.Message.ToLower();
                if (message.Contains("timeout") || message.Contains("connection"))
                    return true;

                // Check status code if available (.NET 5+)
                if (httpEx.StatusCode.HasValue)
                {
                    return httpEx.StatusCode.Value is
                        System.Net.HttpStatusCode.RequestTimeout or          // 408
                        System.Net.HttpStatusCode.TooManyRequests or         // 429
                        System.Net.HttpStatusCode.InternalServerError or     // 500
                        System.Net.HttpStatusCode.BadGateway or              // 502
                        System.Net.HttpStatusCode.ServiceUnavailable or      // 503
                        System.Net.HttpStatusCode.GatewayTimeout;            // 504
                }

                return true; // Generally safe to retry HTTP exceptions
            }

            // I/O exceptions
            if (ex is IOException ioEx)
            {
                var message = ioEx.Message.ToLower();
                return !message.Contains("not find") && !message.Contains("denied");
            }

            // Timeout exceptions
            if (ex is TimeoutException)
                return true;

            // Task/Operation cancelled (but not by user)
            if (ex is TaskCanceledException or OperationCanceledException)
            {
                // Only retry if not user-cancelled
                return ex is not OperationCanceledException opCancelled
                    || !opCancelled.CancellationToken.IsCancellationRequested;
            }

            return false;
        }

        // Commenting as this isn't needed in the test scope
        //private static bool IsDatabaseTransient(Exception ex)
        //{
        //    // EF Core DbUpdateException
        //    if (ex is DbUpdateException dbUpdateEx)
        //    {
        //        if (dbUpdateEx.InnerException is DbException innerDb)
        //            return IsDbExceptionTransient(innerDb);

        //        var message = dbUpdateEx.Message.ToLower();
        //        return message.Contains("timeout")
        //            || message.Contains("deadlock")
        //            || message.Contains("connection")
        //            || message.Contains("transport");
        //    }

        //    // Generic DbException
        //    if (ex is DbException dbEx)
        //        return IsDbExceptionTransient(dbEx);

        //    return false;
        //}

        private static bool IsDbExceptionTransient(DbException dbEx)
        {
            var message = dbEx.Message.ToLower();

            // SQL Server specific error codes
            if (dbEx.GetType().Name.Contains("SqlException"))
            {
                return dbEx.ErrorCode switch
                {
                    -2 => true,      // Timeout
                    -1 => true,      // Connection broken
                    2 => true,       // Network error
                    53 => true,      // Connection broken
                    64 => true,      // Connection issue
                    233 => true,     // Connection initialization error
                    1205 => true,    // Deadlock
                    40197 => true,   // Service error (Azure)
                    40501 => true,   // Service busy (Azure)
                    40613 => true,   // Database unavailable (Azure)
                    _ => false
                };
            }

            // PostgreSQL specific
            if (dbEx.GetType().Name.Contains("NpgsqlException"))
            {
                return message.Contains("timeout")
                    || message.Contains("connection")
                    || message.Contains("deadlock")
                    || message.Contains("serialization failure");
            }

            // MySQL specific
            if (dbEx.GetType().Name.Contains("MySqlException"))
            {
                return dbEx.ErrorCode switch
                {
                    1040 => true,    // Too many connections
                    1205 => true,    // Lock wait timeout
                    1213 => true,    // Deadlock
                    2002 => true,    // Connection refused
                    2003 => true,    // Can't connect
                    2006 => true,    // Server gone away
                    2013 => true,    // Lost connection
                    _ => false
                };
            }

            // Generic fallback
            return message.Contains("timeout")
                || message.Contains("deadlock")
                || message.Contains("connection")
                || message.Contains("transport");
        }
    }
}
