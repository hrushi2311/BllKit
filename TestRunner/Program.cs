using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BllKit.Execution;
using BllKit.Guards;
using BllKit.Models;

class TestRunner
{
    static int _passed = 0;
    static int _failed = 0;

    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║        BllKit Test Runner            ║");
        Console.WriteLine("╚══════════════════════════════════════╝\n");

        // BllResponse Tests
        Console.WriteLine("── BllResponse Tests ──────────────────");
        Test("Success() returns Success status",     () => Assert(BllResponse.Success().Status == BllStatus.Success));
        Test("Success() sets IsSuccess = true",      () => Assert(BllResponse.Success().IsSuccess));
        Test("Success() sets HttpStatusCode = 200",  () => Assert(BllResponse.Success().HttpStatusCode == 200));
        Test("Success(data) attaches data",          () => Assert(BllResponse.Success("hello").Data?.ToString() == "hello"));
        Test("Fail() returns Fail status",           () => Assert(BllResponse.Fail("msg").Status == BllStatus.Fail));
        Test("Fail() sets IsSuccess = false",        () => Assert(!BllResponse.Fail("msg").IsSuccess));
        Test("Fail() with 404 sets correct code",    () => Assert(BllResponse.NotFound("x").HttpStatusCode == 404));
        Test("Forbidden() sets 403",                 () => Assert(BllResponse.Forbidden("x").HttpStatusCode == 403));
        Test("BadRequest() sets 400",                () => Assert(BllResponse.BadRequest("x").HttpStatusCode == 400));
        Test("Error() returns Error status",         () => Assert(BllResponse.Error().Status == BllStatus.Error));
        Test("Error() sets 500 by default",          () => Assert(BllResponse.Error().HttpStatusCode == 500));
        Test("Fail() with errorCode",                () => Assert(BllResponse.Fail("msg","ERR_001",404).ErrorCode == "ERR_001"));
        Test("WithData() attaches data fluently",    () => Assert(BllResponse.Success().WithData(42).Data?.ToString() == "42"));

        // BllResponse<T> Tests
        Console.WriteLine("\n── BllResponse<T> Tests ───────────────");
        Test("Generic Success with typed data",      () => Assert(BllResponse<string>.Success("typed").Data == "typed"));
        Test("Generic Fail returns Fail status",     () => Assert(BllResponse<string>.Fail("msg").Status == BllStatus.Fail));
        Test("Generic NotFound returns 404",         () => Assert(BllResponse<string>.NotFound("x").HttpStatusCode == 404));
        Test("ToUntyped preserves success data",     () => { var r = BllResponse<string>.Success("hi").ToUntyped(); Assert(r.Data?.ToString() == "hi"); });
        Test("ToUntyped preserves errorCode",        () => { var r = BllResponse<string>.Fail("msg","CODE",404).ToUntyped(); Assert(r.ErrorCode == "CODE"); });

        // BllGuard Tests
        Console.WriteLine("\n── BllGuard Tests ──────────────────────");
        Test("EnsureNotNull passes for non-null",    () => { BllGuard.EnsureNotNull("val"); Pass(); });
        Test("EnsureNotNull throws for null",        () => AssertThrows(() => BllGuard.EnsureNotNull(null, "msg", 404, "ERR"), "ERR", 404));
        Test("EnsureNotNullOrEmpty passes",          () => { BllGuard.EnsureNotNullOrEmpty("hello"); Pass(); });
        Test("EnsureNotNullOrEmpty throws empty",    () => AssertThrowsGuard(() => BllGuard.EnsureNotNullOrEmpty("")));
        Test("EnsureNotNullOrEmpty throws spaces",   () => AssertThrowsGuard(() => BllGuard.EnsureNotNullOrEmpty("   ")));
        Test("EnsureNotEmpty passes with items",     () => { BllGuard.EnsureNotEmpty(new List<int>{1,2}); Pass(); });
        Test("EnsureNotEmpty throws empty list",     () => AssertThrowsGuard(() => BllGuard.EnsureNotEmpty(new List<int>())));
        Test("EnsureNotEmpty throws null list",      () => AssertThrowsGuard(() => BllGuard.EnsureNotEmpty((List<int>?)null)));
        Test("EnsureTrue passes when true",          () => { BllGuard.EnsureTrue(true); Pass(); });
        Test("EnsureTrue throws when false",         () => AssertThrowsGuard(() => BllGuard.EnsureTrue(false)));
        Test("EnsureFalse passes when false",        () => { BllGuard.EnsureFalse(false); Pass(); });
        Test("EnsureFalse throws when true",         () => AssertThrowsGuard(() => BllGuard.EnsureFalse(true)));
        Test("EnsureDateRangeValid passes valid",    () => { BllGuard.EnsureDateRangeValid(new DateTime(2025,1,1), new DateTime(2025,12,31)); Pass(); });
        Test("EnsureDateRangeValid throws invalid",  () => AssertThrowsGuard(() => BllGuard.EnsureDateRangeValid(new DateTime(2025,12,1), new DateTime(2025,1,1))));
        Test("EnsureDateRangeValid passes nulls",    () => { BllGuard.EnsureDateRangeValid(null, null); Pass(); });
        Test("EnsureDateRangeProvided throws null",  () => AssertThrowsGuard(() => BllGuard.EnsureDateRangeProvided(DateTime.Now, null)));
        Test("EnsurePositive passes for 1",          () => { BllGuard.EnsurePositive(1); Pass(); });
        Test("EnsurePositive throws for 0",          () => AssertThrowsGuard(() => BllGuard.EnsurePositive(0)));
        Test("EnsurePositive throws for negative",   () => AssertThrowsGuard(() => BllGuard.EnsurePositive(-5)));
        Test("EnsureInRange passes in range",        () => { BllGuard.EnsureInRange(3, 1, 5); Pass(); });
        Test("EnsureInRange throws out of range",    () => AssertThrowsGuard(() => BllGuard.EnsureInRange(10, 1, 5)));

        // BllExecutor Sync Tests
        Console.WriteLine("\n── BllExecutor Sync Tests ──────────────");
        Test("TryExecute returns success",           () => Assert(BllExecutor.TryExecute(() => (object?)"data").IsSuccess));
        Test("TryExecute attaches data",             () => Assert(BllExecutor.TryExecute(() => (object?)"result").Data?.ToString() == "result"));
        Test("TryExecute guard → Fail (not Error)",  () => Assert(BllExecutor.TryExecute(() => { BllGuard.EnsureNotNull(null,"msg",404); return (object?)null; }).Status == BllStatus.Fail));
        Test("TryExecute guard → 404",               () => Assert(BllExecutor.TryExecute(() => { BllGuard.EnsureNotNull(null,"msg",404); return (object?)null; }).HttpStatusCode == 404));
        Test("TryExecute guard → no logger call",    () => { bool called = false; BllExecutor.TryExecute(() => { BllGuard.EnsureNotNull(null); return (object?)null; }, ex => called = true); Assert(!called); });
        Test("TryExecute exception → Error",         () => { bool called = false; var r = BllExecutor.TryExecute(() => { throw new Exception("boom"); return (object?)null; }, ex => called = true); Assert(r.Status == BllStatus.Error && called); });
        Test("TryExecute Action succeeds",           () => Assert(BllExecutor.TryExecute(() => { }).IsSuccess));
        Test("TryExecute<T> returns typed data",     () => Assert(BllExecutor.TryExecute<string>(() => "typed").Data == "typed"));

        // BllExecutor Async Tests
        Console.WriteLine("\n── BllExecutor Async Tests ─────────────");
        await TestAsync("TryExecuteAsync returns success",      async () => Assert((await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); return (object?)"ok"; })).IsSuccess));
        await TestAsync("TryExecuteAsync attaches data",        async () => Assert((await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); return (object?)"data"; })).Data?.ToString() == "data"));
        await TestAsync("TryExecuteAsync guard → Fail",         async () => Assert((await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); BllGuard.EnsureNotNull(null,"msg",404,"CODE"); return (object?)null; })).Status == BllStatus.Fail));
        await TestAsync("TryExecuteAsync guard → 404",          async () => Assert((await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); BllGuard.EnsureNotNull(null,"msg",404); return (object?)null; })).HttpStatusCode == 404));
        await TestAsync("TryExecuteAsync guard → errorCode",    async () => Assert((await BllExecutor.TryExecuteAsync(async () => { BllGuard.EnsureNotNull(null,"msg",404,"ORG_001"); return (object?)null; })).ErrorCode == "ORG_001"));
        await TestAsync("TryExecuteAsync guard → no logger",    async () => { bool called = false; await BllExecutor.TryExecuteAsync(async () => { BllGuard.EnsureTrue(false); return (object?)null; }, ex => called = true); Assert(!called); });
        await TestAsync("TryExecuteAsync exception → Error",    async () => { bool called = false; var r = await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); throw new Exception("boom"); return (object?)null; }, ex => called = true); Assert(r.Status == BllStatus.Error && called); });
        await TestAsync("TryExecuteAsync void succeeds",        async () => Assert((await BllExecutor.TryExecuteAsync(async () => { await Task.Delay(1); })).IsSuccess));
        await TestAsync("TryExecuteAsync<T> typed success",     async () => { var r = await BllExecutor.TryExecuteAsync<List<string>>(async () => { await Task.Delay(1); return new List<string>{"a","b"}; }); Assert(r.IsSuccess && r.Data!.Count == 2); });

        // OQSHA Real-World Scenarios
        Console.WriteLine("\n── OQSHA Real-World Scenarios ──────────");
        await TestAsync("Org null → NotFound response",
            async () => {
                object? org = null;
                var r = await BllExecutor.TryExecuteAsync(async () => {
                    BllGuard.EnsureNotNull(org, "Organisation not found", 404, "ORG_NOT_FOUND");
                    await Task.Delay(1);
                    return (object?)new { data = "alerts" };
                });
                Assert(r.Status == BllStatus.Fail && r.HttpStatusCode == 404 && r.ErrorCode == "ORG_NOT_FOUND");
            });

        await TestAsync("Invalid date range → BadRequest",
            async () => {
                var r = await BllExecutor.TryExecuteAsync(async () => {
                    BllGuard.EnsureDateRangeValid(new DateTime(2025,12,1), new DateTime(2025,1,1), "Invalid dates", 400);
                    await Task.Delay(1);
                    return (object?)null;
                });
                Assert(r.Status == BllStatus.Fail && r.HttpStatusCode == 400);
            });

        await TestAsync("Happy path → Success with data",
            async () => {
                object? org = new { Id = 1, Name = "Osmosys" };
                var r = await BllExecutor.TryExecuteAsync(async () => {
                    BllGuard.EnsureNotNull(org, "Org not found", 404);
                    BllGuard.EnsureDateRangeValid(new DateTime(2025,1,1), new DateTime(2025,12,31));
                    BllGuard.EnsurePositive(1, "OrgId required");
                    await Task.Delay(1);
                    return (object?)new { SafetyAlerts = new[]{"alert1","alert2"} };
                });
                Assert(r.IsSuccess && r.Data != null);
            });

        await TestAsync("DB exception → Error + logger called",
            async () => {
                bool loggerCalled = false;
                var r = await BllExecutor.TryExecuteAsync(async () => {
                    await Task.Delay(1);
                    throw new InvalidOperationException("DB connection failed");
                    return (object?)null;
                }, ex => loggerCalled = true);
                Assert(r.Status == BllStatus.Error && loggerCalled);
            });

        await TestAsync("Empty site list → guard catches it",
            async () => {
                var siteIds = new List<int>();
                var r = await BllExecutor.TryExecuteAsync(async () => {
                    BllGuard.EnsureNotEmpty(siteIds, "No accessible sites", 404);
                    await Task.Delay(1);
                    return (object?)null;
                });
                Assert(r.Status == BllStatus.Fail && r.HttpStatusCode == 404);
            });

        // Summary
        Console.WriteLine("\n╔══════════════════════════════════════╗");
        Console.WriteLine($"║  Results: {_passed} passed, {_failed} failed           ║");
        Console.WriteLine("╚══════════════════════════════════════╝");

        if (_failed > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ {_failed} test(s) FAILED");
            Console.ResetColor();
            Environment.Exit(1);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ All {_passed} tests PASSED!");
            Console.ResetColor();
        }
    }

    static void Test(string name, Action test)
    {
        try { test(); Console.WriteLine($"  ✅ {name}"); _passed++; }
        catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  ❌ {name}: {ex.Message}"); Console.ResetColor(); _failed++; }
    }

    static async Task TestAsync(string name, Func<Task> test)
    {
        try { await test(); Console.WriteLine($"  ✅ {name}"); _passed++; }
        catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  ❌ {name}: {ex.Message}"); Console.ResetColor(); _failed++; }
    }

    static void Assert(bool condition, string? msg = null)
    {
        if (!condition) throw new Exception(msg ?? "Assertion failed");
    }

    static void Pass() { /* explicit pass */ }

    static void AssertThrows(Action action, string? expectedErrorCode, int? expectedHttpStatus)
    {
        try { action(); throw new Exception("Expected BllGuardException but no exception was thrown"); }
        catch (BllGuardException ex)
        {
            if (expectedErrorCode != null && ex.ErrorCode != expectedErrorCode)
                throw new Exception($"Expected ErrorCode '{expectedErrorCode}' but got '{ex.ErrorCode}'");
            if (expectedHttpStatus.HasValue && ex.HttpStatusCode != expectedHttpStatus.Value)
                throw new Exception($"Expected HttpStatus {expectedHttpStatus} but got {ex.HttpStatusCode}");
        }
    }

    static void AssertThrowsGuard(Action action)
    {
        try { action(); throw new Exception("Expected BllGuardException but no exception was thrown"); }
        catch (BllGuardException) { /* expected */ }
    }
}
