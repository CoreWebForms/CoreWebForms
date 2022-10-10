// MIT License.

using System.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer()
    .WrapAspNetCoreSession()
    .AddWebForms()
    .AddDynamicPages(options =>
    {
        options.AddTypeNamespace(typeof(ScriptManager), "asp");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseSystemWebAdapters();
app.UseWebForms();

app.MapAspxPages();
app.MapDynamicAspxPages(new ExcludeObjBinDirectory(app.Environment.ContentRootFileProvider));

app.Run();
