using System;

namespace BllKit.Guards
{
    /// <summary>
    /// Exception thrown by <see cref="BllGuard"/> methods when a guard condition fails.
    /// This is NOT an unexpected error — it represents a known business rule violation.
    ///
    /// When thrown inside <c>BllExecutor.TryExecute</c> or <c>TryExecuteAsync</c>,
    /// it is automatically caught and converted into a <c>BllResponse.Fail()</c>
    /// — NOT a <c>BllResponse.Error()</c>.
    ///
    /// This means the custom error handler (your logger) is NOT called for guard failures,
    /// since they are expected business failures, not unexpected errors.
    /// </summary>
    public class BllGuardException : Exception
    {
        /// <summary>Application-level error code (e.g. "ORG_NOT_FOUND").</summary>
        public string? ErrorCode { get; }

        /// <summary>Suggested HTTP status code (e.g. 404, 403, 400).</summary>
        public int HttpStatusCode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="BllGuardException"/>.
        /// </summary>
        /// <param name="message">Human-readable failure message.</param>
        /// <param name="httpStatusCode">Suggested HTTP status code. Default: 400.</param>
        /// <param name="errorCode">Optional application-level error code.</param>
        public BllGuardException(
            string message,
            int httpStatusCode = 400,
            string? errorCode = null)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
            ErrorCode = errorCode;
        }
    }
}
