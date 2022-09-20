// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Loader;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

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
app.UseWebForms();

app.Map("/alcs", () => AssemblyLoadContext.All.Select(a => new { a.Name, Count = a.Assemblies.Count() }).OrderBy(a => a.Name));
app.MapAspxPages();
app.MapDynamicAspxPages(app.Environment.ContentRootFileProvider);

app.Run();
