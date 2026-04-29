# BllKit — Quick Start Guide

## Step 1 — Open in Visual Studio
Double-click `BllKit.sln` to open the solution.

## Step 2 — Update your details
Open `BllKit/BllKit.csproj` and replace:
- `YourName` → your real name
- `YourUsername` → your GitHub username

## Step 3 — Restore & Build
In Visual Studio: Build → Build Solution
OR in terminal:
```
dotnet restore
dotnet build
```

## Step 4 — Run the tests
In terminal from this folder:
```
dotnet run --project TestRunner/TestRunner.csproj
```
You should see: ✅ All 61 tests PASSED!

## Step 5 — Pack
```
dotnet pack BllKit/BllKit.csproj -c Release -o ./nupkg
```

## Step 6 — Publish to NuGet
1. Create free account at https://nuget.org
2. Go to Profile → API Keys → Create → Copy the key
3. Run:
```
dotnet nuget push ./nupkg/BllKit.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

Done! Your package will appear at https://nuget.org/packages/BllKit in ~15 minutes.
