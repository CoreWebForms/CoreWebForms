// MIT License.

using System.Runtime.Loader;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

builder.Services.AddSystemWebAdapters()
    .AddWebForms()
    .AddDynamicPages();

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

app.UseHttpHandlers();
app.UseSystemWebAdapters();
app.Use((ctx, next) =>
{
    // Fix for https://github.com/dotnet/systemweb-adapters/pull/213
    ctx.Features.Set<IRequestBodyPipeFeature>(new FixedRequestBodyPipeFeature(ctx.Features.GetRequiredFeature<IHttpRequestFeature>()));

    return next(ctx);
});
app.UseWebForms();

app.Map("/alcs", () => AssemblyLoadContext.All.Select(a => new { a.Name, Count = a.Assemblies.Count() }).OrderBy(a => a.Name));
app.MapAspxPages();
app.MapDynamicAspxPages(new ExcludeObjBinDirectory(app.Environment.ContentRootFileProvider));

app.Run();
