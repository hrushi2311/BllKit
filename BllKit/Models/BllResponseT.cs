namespace BllKit.Models
{
    /// <summary>
    /// Strongly-typed version of <see cref="BllResponse"/>.
    /// Use this when you want compile-time safety on the Data property.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public class BllResponse<T>
    {
        /// <summary>Gets or sets the status of the operation.</summary>
        public BllStatus Status { get; set; }

        /// <summary>Gets or sets the human-readable message.</summary>
        public string? Message { get; set; }

        /// <summary>Gets or sets the application-level error code.</summary>
        public string? ErrorCode { get; set; }

        /// <summary>Gets or sets the suggested HTTP status code.</summary>
        public int HttpStatusCode { get; set; }

        /// <summary>Gets or sets the strongly-typed data payload.</summary>
        public T? Data { get; set; }

        /// <summary>Returns true if Status is Success.</summary>
        public bool IsSuccess => Status == BllStatus.Success;

        private BllResponse() { }

        // ─── Factory Methods ─────────────────────────────────────────────────

        /// <summary>Creates a successful response with typed data.</summary>
        public static BllResponse<T> Success(T? data = default, string? message = null)
        {
            return new BllResponse<T>
            {
                Status = BllStatus.Success,
                Data = data,
                Message = message,
                HttpStatusCode = 200
            };
        }

        /// <summary>Creates a failure response for known business rule violations.</summary>
        public static BllResponse<T> Fail(
            string message,
            string? errorCode = null,
            int httpStatusCode = 400)
        {
            return new BllResponse<T>
            {
                Status = BllStatus.Fail,
                Message = message,
                ErrorCode = errorCode,
                HttpStatusCode = httpStatusCode
            };
        }

        /// <summary>Creates an error response for unexpected exceptions.</summary>
        public static BllResponse<T> Error(
            string message = "An unexpected error occurred.",
            string? errorCode = null,
            int httpStatusCode = 500)
        {
            return new BllResponse<T>
            {
                Status = BllStatus.Error,
                Message = message,
                ErrorCode = errorCode,
                HttpStatusCode = httpStatusCode
            };
        }

        /// <summary>Creates a 404 Not Found failure response.</summary>
        public static BllResponse<T> NotFound(string message = "Resource not found.", string? errorCode = null)
            => Fail(message, errorCode, 404);

        /// <summary>Creates a 403 Forbidden failure response.</summary>
        public static BllResponse<T> Forbidden(string message = "Access denied.", string? errorCode = null)
            => Fail(message, errorCode, 403);

        /// <summary>Creates a 400 Bad Request failure response.</summary>
        public static BllResponse<T> BadRequest(string message = "Invalid request.", string? errorCode = null)
            => Fail(message, errorCode, 400);

        /// <summary>
        /// Converts to non-generic <see cref="BllResponse"/>.
        /// Useful when you need to return a BllResponse from a method
        /// but built using the generic version internally.
        /// </summary>
        public BllResponse ToUntyped()
        {
            if (IsSuccess)
                return BllResponse.Success(Data, Message);
            if (Status == BllStatus.Fail)
                return BllResponse.Fail(Message ?? string.Empty, ErrorCode, HttpStatusCode);
            return BllResponse.Error(Message ?? "An unexpected error occurred.", ErrorCode, HttpStatusCode);
        }
    }
}
