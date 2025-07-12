var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure HttpClient for API calls
builder.Services.AddHttpClient("ApiClient", client =>
{
	client.DefaultRequestHeaders.Add("User-Agent", "DUPSSystem.Web/1.0");
	// Skip SSL validation for development
	client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
	ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();