using System;

namespace BllKit.Models
{
    /// <summary>
    /// Standardized response object for BLL operations.
    /// Replaces the common BaseResponse/ApiResponse pattern that gets
    /// copy-pasted across every project.
    /// </summary>
    public class BllResponse
    {
        /// <summary>Gets or sets the status of the operation.</summary>
        public BllStatus Status { get; set; }

        /// <summary>Gets or sets the human-readable message.</summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the application-level error code (e.g. "ORG_NOT_FOUND").
        /// Useful for clients to handle specific error cases programmatically.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the suggested HTTP status code.
        /// e.g. 200, 400, 404, 403, 500
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>Gets or sets the response data payload.</summary>
        public object? Data { get; set; }

        /// <summary>
        /// Returns true if Status is Success.
        /// </summary>
        public bool IsSuccess => Status == BllStatus.Success;

        // ─── Private constructor — use static factory methods ───────────────

        private BllResponse() { }

        // ─── Factory Methods ─────────────────────────────────────────────────

        /// <summary>
        /// Creates a successful response with optional data.
        /// </summary>
        public static BllResponse Success(object? data = null, string? message = null)
        {
            return new BllResponse
            {
                Status = BllStatus.Success,
                Data = data,
                Message = message,
                HttpStatusCode = 200
            };
        }

        /// <summary>
        /// Creates a failure response for known business rule violations.
        /// (e.g. entity not found, access denied, validation failed)
        /// </summary>
        public static BllResponse Fail(
            string message,
            string? errorCode = null,
            int httpStatusCode = 400,
            object? data = null)
        {
            return new BllResponse
            {
                Status = BllStatus.Fail,
                Message = message,
                ErrorCode = errorCode,
                HttpStatusCode = httpStatusCode,
                Data = data
            };
        }

        /// <summary>
        /// Creates an error response for unexpected exceptions.
        /// </summary>
        public static BllResponse Error(
            string message = "An unexpected error occurred.",
            string? errorCode = null,
            int httpStatusCode = 500)
        {
            return new BllResponse
            {
                Status = BllStatus.Error,
                Message = message,
                ErrorCode = errorCode,
                HttpStatusCode = httpStatusCode
            };
        }

        /// <summary>
        /// Creates a 404 Not Found failure response.
        /// </summary>
        public static BllResponse NotFound(string message = "Resource not found.", string? errorCode = null)
            => Fail(message, errorCode, 404);

        /// <summary>
        /// Creates a 403 Forbidden failure response.
        /// </summary>
        public static BllResponse Forbidden(string message = "Access denied.", string? errorCode = null)
            => Fail(message, errorCode, 403);

        /// <summary>
        /// Creates a 400 Bad Request failure response.
        /// </summary>
        public static BllResponse BadRequest(string message = "Invalid request.", string? errorCode = null)
            => Fail(message, errorCode, 400);

        /// <summary>
        /// Fluent method — attach or replace data on an existing response.
        /// </summary>
        public BllResponse WithData(object? data)
        {
            Data = data;
            return this;
        }
    }
}
