# WebForms on ASP.NET Core

The goal of this project is to explore building some of the basic building blocks of the WebForms on ASP.NET Core. This will isolate out the actual components needed to build a functional page.

Supported so far:

- `System.Web.IHttpHandler`
- `System.Web.UI.Page`
- `System.Web.UI.HtmlTextWriter`
- `System.Web.UI.HtmlControls.*`
- `System.Web.UI.WebControls.*`
- `System.Web.Routing.*`
- Master pages
- Compilation of `aspx` pages (both VB and C#)
- Binary compatibility with `System.Web.dll` and `System.Web.Extensions.dll` (if the API exists)
- `ScriptManager` and ajax support
- `System.Web.Optimization`

What is *NOT* supported:

- Designer support
- `System.Web` hosting model
- `System.Web` membership model
- Any `System.Web` concept not called out as in scope

This will make use of `Microsoft.AspNetCore.SystemWebAdapters` to provide the `System.Web.HttpContext` that is at the core of the WebForms pipeline.

## Get Started

1. Add a `nuget.config` or update yours to have the ci feed:

    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <packageSources>
        <!--To inherit the global NuGet package sources remove the <clear/> line below -->
        <clear />
        <add key="nuget" value="https://api.nuget.org/v3/index.json" />
        <add key="webforms" value="https://webformsfeed.blob.core.windows.net/feed/index.json" />
      </packageSources>
    </configuration>
    ```

1. Create a .NET 8 project similar to the following with the WebForms SDK:

    ```xml
    <Project Sdk="CoreWebForms.Sdk/0.2.1">

        <PropertyGroup>
            <TargetFrameworks>net8.0</TargetFrameworks>
            <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
            <!-- Optional, but easier to debug at runtime than compile time -->
            <EnableRuntimeAspxCompilation>true</EnableRuntimeAspxCompilation>
        </PropertyGroup>

    </Project>
    ```

1. Add WebForms to your services (this automatically will add the System.Web adapters - this can be configured independently if needed)

    ```cs
        builder.Services.AddSystemWebAdapters()
            .AddPreApplicationStartMethod(false) // Used if you want to run any pre application start methods
            .AddJsonSessionSerializer()
            .AddWrappedAspNetCoreSession()
            .AddRouting()
            .AddWebForms()
            .AddScriptManager() // Remove if you don't use ScriptManager/AJAX
            .AddOptimization() // Remove if you don't use System.Web.Optimization
            .AddDynamicPages() // Remove if you don't have dynamic pages
            .AddCompiledPages();

    builder.Services.AddSession();
    builder.Services.AddDistributedMemoryCache();
    ```

1. Add the System.Web middleware and map endpoints:

    ```cs
    ...
    app.UseSession();
    app.UseSystemWebAdapters();
    ...
    app.MapHttpHandlers(); // Required for pages (or other configured handlers)
    app.MapScriptManager(); // Required if you want to use ScriptManager
    app.MapBundleTable(); // Required if you want to use BundleTable.Bundles
    ```

1. Add any `.aspx` or `.aspx.cs`/`.aspx.vb` files to your project. They should be served up as expected when you run.

## Samples

For samples, please go to [the samples repo](https://github.com/CoreWebForms/Samples) for up-to-date examples.

## Design docs

Please see [the design docs](./docs/) to see the designs and plans for how this are expected to work.
