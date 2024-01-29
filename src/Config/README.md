# Incremental Configuration Migration

A lot of code uses `System.Configuration.ConfigurationManager.[AppSettings|ConnectionStrings]` and migrating away from that may be difficult. This is somewhat similar to `HttpContext.Current` as it is a static property that had always been available, and now the option to replace it requires rearchitecting things to use `IOptions<>` or some other pattern to propagate configuration.

As applications started to move to the cloud, `ConfigurationManager` didn't work as well as it expected all values to come from local sources. With .NET 4.7.1, .NET Framework added [configuration buidlers](https://learn.microsoft.com/aspnet/config-builder) that provide a way to populate the values of `ConfigurationManager` from different sources. This infrastructure is not available in the .NET Core implementation of `System.Configuration.ConfigurationManager`.

## Potential directions

There are a number of potential directions we could go:

1. Bring over configuration builders to .NET Core. This is potentially a lot of code that would mimic what `IConfigurationSource` does, but in a different way.
2. Provide analyzers/codefixers to identify and refactor code using `ConfigurationManager`. This is difficult as the static `AppSettings`/`ConnectionStrings` doesn't have an analog and would probably require a sizeable refactoring to support.
3. Enable `IConfiguration` and `ConfigurationManager` to stay in sync of what values they are exposing.

## Design Goals

This proposal is built on the following principles to enable an incremental configuration migration:

1. Values must be able to be accessed via `ConfigurationManager.[AppSettings|ConnectionStrings]` so legacy code continues to function
2. Values must be accessible via `IConfiguration` to facilitate migration away from `ConfigurationManager`
3. Values must be able to be updated externally to the app (i.e. environment variables, AppService, etc)

The following are commonly discussed, but violate at least one of these principles:

- *Recommending replacing `web.config` with `appsettings.json`*: This can solve (2) and (3), but fails to satisfy (1)
- *Support `web.config` being loaded by `ConfigurationManager`*: By default, this file is not loaded, and must be renamed to `[main-assembly].dll.config` to be loaded. This can solve (1), but does not satisfy (2) or (3)
- *Support `web.config` as an `IConfiguration` source*: This achieves (2) and (3), but fails to satisfy (1)
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

builder.ConfigureWebConfig();
```

## Challenges

There are a few challenges with this:

- `System.Configuration.ConfigurationManager.ConnectionStrings` is readonly. The POC works around this by some reflection trickery, but we would want to handle this better
- If you link a file (i.e. `web.config`) from a separate project into the core project, it will work fine on publish. However, on F5 debug it will not find the `web.config` file because the content root is the project directory when debugging and not the published output.
