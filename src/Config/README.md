# Configuration Adapter

A lot of code uses `System.Configuration.ConfigurationManager.[AppSettings|ConnectionStrings]` and migrating away from that may be difficult. This is somewhat similar to `HttpContext.Current` as it is a static property that had always been available, and now the option to replace it requires rearchitecting things to use `IOptions<>` or some other pattern to propagate configuration.

## Design Goals

There are a few guiding principles here that need to be kept in mind for an E2E to work:

1. Values must be able to be accessed via `ConfigurationManager.[AppSettings|ConnectionStrings]`
2. Values must be able to be updated externally to the app (i.e. environment variables, AppService, etc)
3. Values must be accessible via `IConfiguration` to facilitate migration away from `ConfigurationManager`

The following are not great options as they violate one of these principles:

- *Recommending replacing `web.config` with `appsettings.json`*: This can solve (2) and (3), but fails to satisfy (1)
- *Support `web.config` as an `IConfiguration` source*: This achieves (1) and (3), but fails to satisfy (2)
- *Continue using `ConfigurationManager` where you are, but update incrementally to `IConfiguration`*: This fails all 3 principles

## Proposal

A migration path can be created by combining the recommendations into a single approach:

1. We add support for `web.config`/`app.config` as a source for `IConfiguration`
1. We add support to update `ConfigurationManager` entries with values from `IConfiguration` (and potentially support reloading)

By supporting these two scenarios, we can enable users to use their current set up, but also have a pathway to migrate to `IConfiguration` as the data in both of the configuration storage systems will be the same.

## Example

An example of how this could work is available in this repo. The user then would do the following:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddWebConfig();
builder.Services.AddConfigurationManager();
```

There are two APIs that users would need to add:

- `IConfigurationBuilder.AddWebConfig()`: This would add `web.config` as a source for `IConfiguration`
- `IServiceCollection.AddConfigurationManager()`: This would add an `IStartupFilter` that would take the `IConfiguration` and update values in `ConfigurationManager` and optionally reload the values if `IConfiguration` is reloaded

## Challenges

There are a few challenges with this:

- `System.Configuration.ConfigurationManager.ConnectionStrings` is readonly. The POC works around this by some reflection trickery, but we would want to handle this better
- If you link a file (i.e. `web.config`) from a separate project into the core project, it will work fine on publish. However, on F5 debug it will not find the `web.config` file because the content root is the project directory when debugging and not the published output.
