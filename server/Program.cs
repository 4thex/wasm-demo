// Serve ../wwwroot (the page, sw.js and math.wasm) from the same origin as the API.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "../wwwroot" });
builder.Services.AddSingleton<IMath, MathWasm>();
builder.Services.AddSingleton<AddTwoHandler>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/add-two", (HttpRequest request, AddTwoHandler handler) => handler.Handle(request))
   .AddEndpointFilter<MathFilter>();

app.Run("http://localhost:5000");
