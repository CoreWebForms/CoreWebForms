// MIT License.

using System.Runtime.Loader;
using System.Security.Claims;
using System.Web.Optimization;

var builder = WebApplication.CreateBuilder(args);

builder.UseWebConfig(isOptional: false);

builder.Services.AddDataProtection();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSystemWebAdapters()
    .AddPreApplicationStartMethod()
    .AddJsonSessionSerializer()
    .AddWrappedAspNetCoreSession()
    .AddRouting()
    .AddWebForms()
    .AddScriptManager()
    .AddOptimization(bundles =>
    {
        bundles.Add(new ScriptBundle("~/scriptbundle")
            .Include("~/script.js"));
    })
#if WEBFORMS_DYNAMIC
    .AddDynamicPages(options =>
    {
        options.RegisterPrefix("webopt", "Microsoft.AspNet.Web.Optimization.WebForms", "WebForms.Optimization");
        options.RegisterPrefix("webopt", "System.Web.Optimization", "WebForms.Optimization");
    });
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

// Add a fake user so things like WebParts has an authenticated user
app.Use((ctx, next) =>
{
    if (ctx.User is not { Identity.IsAuthenticated: true })
    {
        ctx.User = new ClaimsPrincipal([new ClaimsIdentity([new Claim(ClaimTypes.Name, "myName")], "LocalAuth", ClaimTypes.Name, ClaimTypes.Role)]);
    }
    return next(ctx);
});

app.UseSession();
app.UseSystemWebAdapters();

app.MapGet("/acls", () => AssemblyLoadContext.All.Select(acl => new
{
    Name = acl.Name,
    Assemblies = acl.Assemblies.Select(a => a.FullName)
}));

app.MapHttpHandlers();
app.MapScriptManager();
app.MapBundleTable();

app.Run();
