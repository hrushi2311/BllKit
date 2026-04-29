using System;
using System.Threading.Tasks;
using BllKit.Guards;
using BllKit.Models;

namespace BllKit.Execution
{
    /// <summary>
    /// Executes BLL operations with automatic exception handling,
    /// eliminating the repetitive try/catch/BaseResponse boilerplate
    /// found in every BLL method.
    ///
    /// <para>
    /// How it works:
    /// <list type="bullet">
    ///   <item>If the delegate succeeds → returns <c>BllResponse.Success(data)</c></item>
    ///   <item>If a <see cref="BllGuardException"/> is thrown → returns <c>BllResponse.Fail()</c> (does NOT call error handler)</item>
    ///   <item>If any other exception is thrown → calls your error handler, returns <c>BllResponse.Error()</c></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Before (40 lines):
    /// <code>
    /// BaseResponse response = new BaseResponse(ResponseStatus.Success);
    /// try
    /// {
    ///     var org = await orgRepo.GetOrgById(orgId);
    ///     if (org == null) { response.Status = Fail; response.HttpStatusCode = 404; return response; }
    ///     response.Data = await _repo.GetDataAsync(orgId);
    /// }
    /// catch (Exception ex) { LogError(ex); response.Status = Error; }
    /// return response;
    /// </code>
    /// </para>
    ///
    /// <para>
    /// After (6 lines):
    /// <code>
    /// return await BllExecutor.TryExecuteAsync(async () =>
    /// {
    ///     BllGuard.EnsureNotNull(org, "Organisation not found", 404, "ORG_NOT_FOUND");
    ///     return await _repo.GetDataAsync(orgId);
    /// }, ex => this.LogError(ex));
    /// </code>
    /// </para>
    /// </summary>
    public static class BllExecutor
    {
        // ═══════════════════════════════════════════════════════════════════════
        // ASYNC — TryExecuteAsync returning BllResponse (non-generic)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Executes an async operation and returns a <see cref="BllResponse"/>.
        /// </summary>
        /// <param name="operation">Your business logic. Return the data object or null.</param>
        /// <param name="onError">Optional handler called when an unexpected exception occurs (use for logging).</param>
        /// <param name="unexpectedErrorMessage">Message used in the Error response. Defaults to a generic message.</param>
        public static async Task<BllResponse> TryExecuteAsync(
            Func<Task<object?>> operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                object? data = await operation().ConfigureAwait(false);
                return BllResponse.Success(data);
            }
            catch (BllGuardException guardEx)
            {
                // Known business rule violation — return Fail, do NOT call error handler
                return BllResponse.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                // Unexpected error — call error handler (logger) and return Error
                onError?.Invoke(ex);
                return BllResponse.Error(unexpectedErrorMessage);
            }
        }

        /// <summary>
        /// Executes an async operation that returns void/no data, returns a <see cref="BllResponse"/>.
        /// </summary>
        public static async Task<BllResponse> TryExecuteAsync(
            Func<Task> operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                await operation().ConfigureAwait(false);
                return BllResponse.Success();
            }
            catch (BllGuardException guardEx)
            {
                return BllResponse.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return BllResponse.Error(unexpectedErrorMessage);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ASYNC — TryExecuteAsync returning BllResponse<T> (generic)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Executes an async operation and returns a strongly-typed <see cref="BllResponse{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the data returned from the operation.</typeparam>
        /// <param name="operation">Your business logic. Return your typed data.</param>
        /// <param name="onError">Optional handler called when an unexpected exception occurs.</param>
        /// <param name="unexpectedErrorMessage">Message used in the Error response.</param>
        public static async Task<BllResponse<T>> TryExecuteAsync<T>(
            Func<Task<T>> operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                T data = await operation().ConfigureAwait(false);
                return BllResponse<T>.Success(data);
            }
            catch (BllGuardException guardEx)
            {
                return BllResponse<T>.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return BllResponse<T>.Error(unexpectedErrorMessage);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SYNC — TryExecute returning BllResponse (non-generic)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Executes a synchronous operation and returns a <see cref="BllResponse"/>.
        /// </summary>
        /// <param name="operation">Your business logic. Return the data object or null.</param>
        /// <param name="onError">Optional handler called when an unexpected exception occurs.</param>
        /// <param name="unexpectedErrorMessage">Message used in the Error response.</param>
        public static BllResponse TryExecute(
            Func<object?> operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                object? data = operation();
                return BllResponse.Success(data);
            }
            catch (BllGuardException guardEx)
            {
                return BllResponse.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return BllResponse.Error(unexpectedErrorMessage);
            }
        }

        /// <summary>
        /// Executes a synchronous operation that returns void/no data.
        /// </summary>
        public static BllResponse TryExecute(
            Action operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                operation();
                return BllResponse.Success();
            }
            catch (BllGuardException guardEx)
            {
                return BllResponse.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return BllResponse.Error(unexpectedErrorMessage);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SYNC — TryExecute returning BllResponse<T> (generic)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Executes a synchronous operation and returns a strongly-typed <see cref="BllResponse{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the data returned from the operation.</typeparam>
        public static BllResponse<T> TryExecute<T>(
            Func<T> operation,
            Action<Exception>? onError = null,
            string unexpectedErrorMessage = "An unexpected error occurred.")
        {
            try
            {
                T data = operation();
                return BllResponse<T>.Success(data);
            }
            catch (BllGuardException guardEx)
            {
                return BllResponse<T>.Fail(guardEx.Message, guardEx.ErrorCode, guardEx.HttpStatusCode);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return BllResponse<T>.Error(unexpectedErrorMessage);
            }
        }
    }
}
