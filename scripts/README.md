# Scripts

This folder contains C# standalone automation scripts following ADR 0014.

Each script lives in its own subfolder (`<verb-noun>/`) with a `Program.cs` entry point and an optional `.csproj` sidecar for NuGet dependencies.

Shared helper code is in `_shared/`.

## Running a script

```bash
dotnet run --project scripts/<script-name>
```

See the comment header in each `Program.cs` for prerequisites and argument details.
