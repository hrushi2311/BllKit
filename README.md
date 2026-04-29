# BllKit

A lightweight **Business Logic Layer toolkit** for .NET that eliminates repetitive `try/catch` boilerplate, standardizes API responses, and provides fluent guard validations.

Compatible with **.NET Framework 4.5.2+**, **.NET Core**, **.NET 5/6/7/8**.

---

## The Problem

Every .NET BLL method ends up looking like this:

```csharp
// 40 lines — but only 5 lines are actual logic
public async Task<BaseResponse> GetSafetyAlertsByMonth(int orgId, DashboardReportFilter filter)
{
    BaseResponse response = new BaseResponse(ResponseStatus.Success);
    try
    {
        OrganisationDBO organisation = await new OrganisationRepo(_conn).GetOrgById(orgId);
        if (organisation == null)
        {
            response.Status = ResponseStatus.Fail;
            response.Message = "Organisation not found";
            response.HttpStatusCode = 404;
            return response;
        }
        if (filter.FromDate > filter.ToDate)
        {
            response.Status = ResponseStatus.Error;
            response.Message = "Invalid date range";
            return response;
        }
        // actual logic (5 lines)
        response.Data = await _repo.GetAlertsAsync(orgId, filter);
    }
    catch (Exception ex)
    {
        this.LogError(ex);
        response.Status = ResponseStatus.Error;
        response.Message = "An unexpected error occurred.";
    }
    return response;
}
```

**BllKit fixes this.**

---

## The Solution

```csharp
// 8 lines — same result
public async Task<BllResponse> GetSafetyAlertsByMonth(int orgId, DashboardReportFilter filter)
{
    return await BllExecutor.TryExecuteAsync(async () =>
    {
        BllGuard.EnsureNotNull(organisation, "Organisation not found", 404, "ORG_NOT_FOUND");
        BllGuard.EnsureDateRangeValid(filter.FromDate, filter.ToDate);
        return await _repo.GetAlertsAsync(orgId, filter);
    }, ex => this.LogError(ex));
}
```

---

## Installation

```bash
dotnet add package BllKit
```

---

## Quick Start

### 1. BllResponse — Standardized Response Model

```csharp
// Success with data
BllResponse.Success(data);
BllResponse.Success(data, "Records loaded");

// Known business failures
BllResponse.Fail("Organisation not found", errorCode: "ORG_NOT_FOUND", httpStatusCode: 404);
BllResponse.NotFound("Organisation not found", "ORG_NOT_FOUND");
BllResponse.Forbidden("Access denied", "NO_ACCESS");
BllResponse.BadRequest("OrgId is required");

// Unexpected errors
BllResponse.Error("Something went wrong");

// Strongly typed
BllResponse<List<OrderDto>>.Success(orders);
BllResponse<List<OrderDto>>.NotFound("No orders found");
```

### 2. BllGuard — Fluent Validations

Guards throw `BllGuardException` which is automatically caught by `BllExecutor` and converted to a `Fail` response — your logger is **not** called for guard failures.

```csharp
BllGuard.EnsureNotNull(organisation, "Organisation not found", 404, "ORG_NOT_FOUND");
BllGuard.EnsureNotNullOrEmpty(request.Name, "Name is required");
BllGuard.EnsureNotEmpty(siteIds, "No sites found", 404);
BllGuard.EnsureDateRangeValid(filter.FromDate, filter.ToDate);
BllGuard.EnsureDateRangeProvided(filter.FromDate, filter.ToDate);
BllGuard.EnsureTrue(user.HasAccess, "Access denied", 403, "NO_ACCESS");
BllGuard.EnsurePositive(orgId, "OrgId must be greater than 0");
BllGuard.EnsureInRange(reportType, 1, 3, "Invalid report type");
```

### 3. BllExecutor — Try/Catch Wrapper

```csharp
// Async — returns object data
BllResponse response = await BllExecutor.TryExecuteAsync(
    async () => await _repo.GetDataAsync(orgId),
    ex => _logger.Error(ex)   // optional, your logger
);

// Async — strongly typed
BllResponse<List<Alert>> response = await BllExecutor.TryExecuteAsync<List<Alert>>(
    async () => await _repo.GetAlertsAsync(orgId),
    ex => _logger.Error(ex)
);

// Async — no return value
BllResponse response = await BllExecutor.TryExecuteAsync(
    async () => await _repo.DeleteAsync(id),
    ex => _logger.Error(ex)
);

// Sync
BllResponse response = BllExecutor.TryExecute(
    () => _repo.GetData(orgId),
    ex => _logger.Error(ex)
);
```

---

## Response Model

```csharp
public class BllResponse
{
    public BllStatus Status { get; }       // Success | Fail | Error
    public string? Message { get; }
    public string? ErrorCode { get; }      // e.g. "ORG_NOT_FOUND"
    public int HttpStatusCode { get; }     // e.g. 200, 404, 500
    public object? Data { get; }
    public bool IsSuccess { get; }
}
```

---

## How Errors Are Handled

| Scenario | What happens |
|---|---|
| Operation succeeds | Returns `BllResponse.Success(data)` |
| `BllGuard` check fails | Returns `BllResponse.Fail(...)` — logger NOT called |
| Unexpected exception | Calls your error handler, returns `BllResponse.Error()` |

---

## License

MIT
