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
- Binary compatibility (via the `WebForms.SystemWebShim` package) - limited support; needs testing

What is *NOT* supported:

- Designer support
- `System.Web` hosting model
- `System.Web` membership model
- Any `System.Web` concept not called out as in scope

This will make use of `Microsoft.AspNetCore.SystemWebAdapters` to provide the `System.Web.HttpContext` that is at the core of the WebForms pipeline.

## Packages

- `WebForms` - Contains the majority of the page/control/etc methods required for WebForms
- `WebForms.Compiler` - Build time compiler that will generate a `.dll` for each page in the project. This includes a Roslyn code generator that can generate a strongly-type file to include the compiled assemblies that will remove the need for runtime reflection to load them.
- `WebForms.Compiler.Dynamic` - Run time compiler that will allow for updating `aspx` at run time and generating a new in-memory assembly
- `WebForms.HttpHandler` - Contains `IHttpHandler` and related helpers to enable hooking them up to ASP.NET Core
- `WebForms.Routing` - Contains APIs from the `System.Web.Routing` namespace
- `WebForms.SystemWebShim` - A package with a `System.Web.dll` assembly that will type forward to the new locations. This is build for .NET 6+ and would help with controls/assemblies/etc that are compiled and cannot be recompiled for some reason. If they use members/types not supported, they will throw at runtime, but can be a helpful step in migrating old projects (see the `TypeDumper` tool to regenerate the available types for the shim)

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

1. Add `WebForms` and `WebForms.Compiler` to your project
1. Add WebForms to your services (this automatically will add the System.Web adapters - this can be configured independently if needed)

    ```cs
    builder.Services.AddWebForms()
        .AddCompiledPages();
    
    builder.Services.AddSession();
    builder.Services.AddDistributedMemoryCache();
    ```

1. Add the System.Web middleware and map the webforms pages:

    ```cs
    ...
    app.UseSession();
    app.UseSystemWebAdapters();
    ...
    app.MapWebForms();
    ```

1. Add any `.aspx` or `.aspx.cs`/`.aspx.vb` files to your project. They should be served up as expected when you run.

> NOTE: Set the `<UseWebFormsInterceptor>true</UseWebFormsInterceptor>` if you are using C# 12+ and would like to have the static assets compiled into the application instead of using runtime reflection to load them.

### Alternate

There is a dynamic compilation method that can be enabled by doing the following (continued from the above sample):

1. Add `WebForms.Compiler.Dynamic` and `WebForms.Compiler`
1. Add `<EnableRuntimeAspxCompilation>true</EnableRuntimeAspxCompilation>` to a property group in the project file
1. Add dynamic compilation to the services:

    ```csharp
    builder.Services.AddWebForms()
        .AddDynamicPages();
    ``````

## Design docs

Please see [the design docs](./docs/) to see the designs and plans for how this are expected to work.
