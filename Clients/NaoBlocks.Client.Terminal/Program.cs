using NaoBlocks.Client.Terminal;

using var app = new App();
await app.RunAsync(
    new StandardConsole(),
    args);