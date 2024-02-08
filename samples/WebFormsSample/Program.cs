// MIT License.

using System.Runtime.Loader;
using System.Web.UI;
using System.Web.UI.WebControls;

var builder = WebApplication.CreateBuilder(args);

builder.UseWebConfig(isOptional: false);

builder.Services.AddDataProtection();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSystemWebAdapters()
    .AddJsonSessionSerializer()
    .AddWrappedAspNetCoreSession()
    .AddRouting()
    .AddWebForms()
    .AddScriptManager()
#if WEBFORMS_DYNAMIC
    .AddDynamicPages()
    .AddPrefix<ScriptManager>("asp")
    .AddPrefix<ListView>("asp");
#else
    .AddCompiledPages();
#endif

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

app.MapGet("/acls", () => AssemblyLoadContext.All.Select(acl => new
{
    Name = acl.Name,
    Assemblies = acl.Assemblies.Select(a => a.FullName)
}));

app.MapWebForms();
app.MapScriptManager();

app.Run();
