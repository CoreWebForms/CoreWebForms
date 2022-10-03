// MIT License.

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

app.UseSystemWebAdapters();

app.MapAspxPages();
app.MapDynamicAspxPages(new ExcludeObjBinDirectory(app.Environment.ContentRootFileProvider));

app.Run();
