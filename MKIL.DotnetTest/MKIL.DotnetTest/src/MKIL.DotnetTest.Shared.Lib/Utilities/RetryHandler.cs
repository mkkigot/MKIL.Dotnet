using MKIL.DotnetTest.Shared.Lib.Messaging.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKIL.DotnetTest.Shared.Lib.Utilities
{
    public static class RetryHandler
    {
        public sealed record RetryCompletionContext(
        bool Succeeded,
        int Attempts,
        TimeSpan Elapsed,
        Exception? Exception,
        bool Canceled);

        /// <summary>
        /// Executes an action with retry logic for transient errors
        /// </summary>
        public static async Task ExecuteAsync(
            Func<Task> action,
            int maxRetries = 3,
            Action<Exception, int, TimeSpan>? onRetry = null,
            Func<RetryCompletionContext, Task>? onComplete = null,
            CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(
                async () =>
                {
                    await action();
                    return true; // Dummy return value
                },
                maxRetries,
                onRetry,
                onComplete,
                cancellationToken);
        }

        /// <summary>
        /// Executes a function with retry logic for transient errors
        /// </summary>
        public static async Task<T> ExecuteAsync<T>(
            Func<Task<T>> action,
            int maxRetries = 3,
            Action<Exception, int, TimeSpan>? onRetry = null,
            Func<RetryCompletionContext, Task>? onComplete = null,
            CancellationToken cancellationToken = default)
        {
            if (maxRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), "Must be >= 0");

            var attempt = 0;
            Exception? lastException = null;
            var stopwatch = Stopwatch.StartNew();
            var succeeded = false;
            var canceled = false;

            try
            {
                while (attempt <= maxRetries)
                {
                    try
                    {
                        var value = await action();
                        succeeded = true;
                        return value;
                    }
                    catch (Exception ex) when (ShouldRetry(ex, attempt, maxRetries))
                    {
                        lastException = ex;
                        attempt++;

                        var delay = CalculateDelay(attempt);

                        onRetry?.Invoke(ex, attempt, delay);

                        await Task.Delay(delay, cancellationToken);
                    }
                }

                // If we get here, we exhausted all retries
                stopwatch.Stop();
                throw new MaxRetriesExceededException(
                    $"Operation failed after {maxRetries} retry attempts",
                    lastException!,
                    attempt,
                    stopwatch.Elapsed);
            }
            catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
            {
                canceled = true;
                lastException = oce;
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                if (onComplete != null)
                {
                    await onComplete(new RetryCompletionContext(
                        Succeeded: succeeded,
                        Attempts: attempt,
                        Elapsed: stopwatch.Elapsed,
                        Exception: lastException,
                        Canceled: canceled));
                }
            }
        }

        /// <summary>
        /// Calculates delay with exponential backoff and jitter
        /// </summary>
        public static TimeSpan CalculateDelay(int attempt, double baseSeconds = 2.0)
        {
            // Exponential backoff: base^attempt seconds
            var exponentialDelay = TimeSpan.FromSeconds(Math.Pow(baseSeconds, attempt));

            // Add jitter (0-1000ms) to prevent thundering herd
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));

            // Cap at max delay of 30 seconds
            var totalDelay = exponentialDelay + jitter;
            return totalDelay > TimeSpan.FromSeconds(30)
                ? TimeSpan.FromSeconds(30)
                : totalDelay;
        }

        private static bool ShouldRetry(Exception ex, int currentAttempt, int maxRetries)
        {
            // Don't retry if we've exhausted attempts
            if (currentAttempt >= maxRetries)
                return false;

            // Don't retry permanent errors
            if (TransientErrorDetector.IsPermanent(ex))
                return false;

            // Retry transient errors
            return TransientErrorDetector.IsTransient(ex);
        }
    }
}
