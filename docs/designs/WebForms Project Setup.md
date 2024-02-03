# WebForms project set up

WebForms relies on much in the `System.Web` namespace. Therefore, any application using it must also use the [System.Web adapters](https://github.com/dotnet/systemweb-adapters). However, a new builder will be used to add services to the app for WebForms so we can ensure that the required services will be available.

```csharp
public interface IWebFormsBuilder
{
    IServiceCollection Services { get; }

    ISystemWebAdapterBuilder SystemWebAdapterBuilder { get; }
}
```

From the WebForms builder, the services as well as the System.Web adapter builder is accessible if needed.

In order to register services, either of the following patterns is available:

- Simple registration:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register WebForms and initialize System.Web adapters
builder.Services.AddWebForms();

var app = builder.Build();

app.UseSession(); // The default registration will setup System.Web session with ASP.NET Core session and will need this middleware
app.UseSystemwebAdapters();

app.MapWebForms()
    .AddDynamicPages() // If dynamic pages are used
    .AddCompiledPages(); // If compiled pages are used
```

- Custom registration:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters()
    ...
    .AddWebForms()
    .AddDynamicPages() // If dynamic pages are used
    .AddCompiledPages() // If compiled pages are used
    ...;

var app = builder.Build();

app.UseSession(); // If needed with custom setup
app.UseSystemwebAdapters();

app.MapWebForms();
```

The `MapWebForms()` method will map all [handlers](./Handlers.md) as well as set up endpoints required for scripts provided by WebForms.
