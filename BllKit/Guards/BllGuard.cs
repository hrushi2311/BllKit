using System;
using System.Collections.Generic;

namespace BllKit.Guards
{
    /// <summary>
    /// Provides guard clause methods for BLL operations.
    ///
    /// Guards throw <see cref="BllGuardException"/> on failure, which is automatically
    /// caught by <c>BllExecutor.TryExecute/TryExecuteAsync</c> and converted into
    /// a <c>BllResponse.Fail()</c> — so you never need a try/catch for guard failures.
    ///
    /// Usage inside TryExecute:
    /// <code>
    /// return await BllExecutor.TryExecuteAsync(async () =>
    /// {
    ///     BllGuard.EnsureNotNull(organisation, "Organisation not found", 404, "ORG_NOT_FOUND");
    ///     BllGuard.EnsureDateRangeValid(filter.FromDate, filter.ToDate);
    ///     return await _repo.GetDataAsync();
    /// });
    /// </code>
    /// </summary>
    public static class BllGuard
    {
        // ─── Null / Existence Checks ─────────────────────────────────────────

        /// <summary>
        /// Ensures the value is not null.
        /// Throws <see cref="BllGuardException"/> if null.
        /// </summary>
        public static void EnsureNotNull(
            object? value,
            string message = "Resource not found.",
            int httpStatusCode = 404,
            string? errorCode = null)
        {
            if (value == null)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        /// <summary>
        /// Ensures the string is not null or whitespace.
        /// Throws <see cref="BllGuardException"/> if null or empty.
        /// </summary>
        public static void EnsureNotNullOrEmpty(
            string? value,
            string message = "Value is required.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        /// <summary>
        /// Ensures the collection is not null and has at least one item.
        /// Throws <see cref="BllGuardException"/> if null or empty.
        /// </summary>
        public static void EnsureNotEmpty<T>(
            IList<T>? collection,
            string message = "List cannot be empty.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (collection == null || collection.Count == 0)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        // ─── Condition Checks ────────────────────────────────────────────────

        /// <summary>
        /// Ensures the condition is true.
        /// Throws <see cref="BllGuardException"/> if condition is false.
        /// </summary>
        public static void EnsureTrue(
            bool condition,
            string message = "Condition not met.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (!condition)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        /// <summary>
        /// Ensures the condition is false.
        /// Throws <see cref="BllGuardException"/> if condition is true.
        /// </summary>
        public static void EnsureFalse(
            bool condition,
            string message = "Condition must not be met.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (condition)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        // ─── Date Range Checks ───────────────────────────────────────────────

        /// <summary>
        /// Ensures FromDate is not greater than ToDate.
        /// Throws <see cref="BllGuardException"/> if the range is invalid.
        /// </summary>
        public static void EnsureDateRangeValid(
            DateTime? fromDate,
            DateTime? toDate,
            string message = "FromDate cannot be greater than ToDate.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        /// <summary>
        /// Ensures both FromDate and ToDate are provided.
        /// Throws <see cref="BllGuardException"/> if either is null.
        /// </summary>
        public static void EnsureDateRangeProvided(
            DateTime? fromDate,
            DateTime? toDate,
            string message = "FromDate and ToDate are required.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        // ─── Numeric Checks ──────────────────────────────────────────────────

        /// <summary>
        /// Ensures the integer is greater than zero.
        /// Throws <see cref="BllGuardException"/> if value is zero or negative.
        /// </summary>
        public static void EnsurePositive(
            int value,
            string message = "Value must be greater than zero.",
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (value <= 0)
                throw new BllGuardException(message, httpStatusCode, errorCode);
        }

        /// <summary>
        /// Ensures the value is within an inclusive range [min, max].
        /// Throws <see cref="BllGuardException"/> if out of range.
        /// </summary>
        public static void EnsureInRange(
            int value,
            int min,
            int max,
            string? message = null,
            int httpStatusCode = 400,
            string? errorCode = null)
        {
            if (value < min || value > max)
                throw new BllGuardException(
                    message ?? $"Value must be between {min} and {max}.",
                    httpStatusCode,
                    errorCode);
        }
    }
}
