using HiddenGemDAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1 Pull Database Configuration
string dbEndpoint = builder.Configuration["SurrealDb:Endpoint"] ?? "localhost:8000";
string dbUser = builder.Configuration["SurrealDb:User"] ?? "root";
string dbPass = builder.Configuration["SurrealDb:Pass"] ?? "root";

string keywordsPath = builder.Configuration["ResourcePaths:KeywordsJson"]
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "HiddenGemResources", "Keywords.json"));

builder.Services.AddHiddenGemData(dbEndpoint, dbUser, dbPass, keywordsPath);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
