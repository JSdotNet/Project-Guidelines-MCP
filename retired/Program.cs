// The retired package's only behaviour: tell the host where the server moved, then stop.
// stderr, because stdout is the MCP transport; a non-zero exit so the host reports the failure.
await Console.Error.WriteLineAsync(
    "Moved to JSdotNet.AppDefaults.Mcp. This package is retired and serves nothing. " +
    "Replace it with: dnx JSdotNet.AppDefaults.Mcp --yes (server id jsdotnet-app-defaults). " +
    "Setup: https://github.com/JSdotNet/app-defaults/blob/main/docs/install.md");
return 1;
