var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();   // Ca să folosească index.html implicit
app.UseStaticFiles();    // Ca să servească wwwroot

app.MapGet("/hello", () => "Hello from WebApp!");
app.Run();