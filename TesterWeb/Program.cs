var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
var mvc = services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
