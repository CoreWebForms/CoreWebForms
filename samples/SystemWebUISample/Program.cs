// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

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
app.UseWebForms();

app.Map("/alcs", () => AssemblyLoadContext.All.Select(a => new { a.Name, Count = a.Assemblies.Count() }).OrderBy(a => a.Name));
app.MapAspxPages();
app.MapDynamicAspxPages(new ExcludeObjBinDirectory(app.Environment.ContentRootFileProvider));

app.Run();

sealed class ExcludeObjBinDirectory : IFileProvider
{
    private readonly IFileProvider _provider;

    public ExcludeObjBinDirectory(IFileProvider provider)
    {
        _provider = provider;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var contents = _provider.GetDirectoryContents(subpath);

        return string.IsNullOrEmpty(subpath) || subpath == "."
            ? new ExcludeDirectory(contents)
            : contents;
    }

    public IFileInfo GetFileInfo(string subpath) => _provider.GetFileInfo(subpath);

    public IChangeToken Watch(string filter) => _provider.Watch(filter);

    private sealed class ExcludeDirectory : IDirectoryContents
    {
        private readonly IDirectoryContents _contents;

        public ExcludeDirectory(IDirectoryContents contents)
        {
            _contents = contents;
        }

        public bool Exists => _contents.Exists;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var item in _contents)
            {
                if (string.Equals("bin", item.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("obj", item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

