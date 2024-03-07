// MIT License.

using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

[assembly: TagPrefix("System.Web.UI.WebControls.WebParts", "asp")]

namespace WebForms;

internal static class WebPartsServiceExtensions
{
    internal static IWebFormsBuilder AddWebParts(this IWebFormsBuilder builder)
    {
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<PersonalizationProvider, InMemoryPersonalizationProvider>();
        builder.Services.TryAddSingleton<PersonalizationProviderCollection>();
        builder.Services.AddOptions<WebPartsOptions>()
            .Configure(options =>
            {
                options.AllowedCapabilities.Add(WebPartPersonalization.EnterSharedScopeUserCapability.Name);
                options.AllowedCapabilities.Add(WebPartPersonalization.ModifyStateUserCapability.Name);
            });

        return builder;
    }

    /// <summary>
    /// A default personalization provider implementation modeled SqlPersonalizationProvider but using an in-memory cache 
    /// </summary>
    private sealed class InMemoryPersonalizationProvider(IWebHostEnvironment env, TimeProvider time, ILogger<InMemoryPersonalizationProvider> logger) : PersonalizationProvider
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, DataBlob> _sharedCache = [];
        private readonly Dictionary<(string Path, string User), DataBlob> _userCache = [];

        private readonly record struct DataBlob(byte[] Data, DateTimeOffset LastAccessed);

        public override string ApplicationName { get; set; } = env.ApplicationName;

        public override PersonalizationStateInfoCollection FindState(PersonalizationScope scope, PersonalizationStateQuery query, int pageIndex, int pageSize, out int totalRecords)
        {
            logger.LogWarning("DefaultPersonalizationProvider.FindState is not implemented");
            totalRecords = 0;
            return [];
        }

        public override int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query)
        {
            logger.LogWarning("DefaultPersonalizationProvider.GetCountOfState is not implemented");

            lock (_lock)
            {
                return scope is PersonalizationScope.Shared ? _sharedCache.Count : _userCache.Count;
            }
        }

        public override int ResetState(PersonalizationScope scope, string[] paths, string[] usernames)
        {
            return (scope, paths, usernames) switch
            {
                (_, null, null) => ResetState(scope),
                (_, { Length: > 0 }, null) => ResetPathState(scope, paths),
                (PersonalizationScope.User, [{ } singlePath], { Length: > 0 }) => ResetState(singlePath, usernames),
                (PersonalizationScope.User, null, { Length: > 0 }) => ResetUserState(usernames),
                _ => throw new ArgumentException("Invalid arguments for ResetState"),
            };
        }

        private int ResetUserState(string[] users)
        {
            var set = users.ToHashSet(StringComparer.OrdinalIgnoreCase);

            lock (_lock)
            {
                var count = 0;
                foreach (var key in _userCache.Keys)
                {
                    if (set.Contains(key.User))
                    {
                        count++;
                        _userCache.Remove(key);
                    }
                }
                return count;
            }
        }

        private int ResetState(string path, string[] users)
        {
            var set = users.ToHashSet(StringComparer.OrdinalIgnoreCase);

            lock (_lock)
            {
                var count = 0;
                foreach (var key in _userCache.Keys)
                {
                    if (path == key.Path && set.Contains(key.User))
                    {
                        count++;
                        _userCache.Remove(key);
                    }
                }
                return count;
            }
        }

        private int ResetPathState(PersonalizationScope scope, string[] paths)
        {
            lock (_lock)
            {
                var count = 0;
                if (scope is PersonalizationScope.Shared)
                {
                    foreach (var path in paths)
                    {
                        count++;
                        _sharedCache.Remove(path);
                    }
                }
                else
                {
                    var set = paths.ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var key in _userCache.Keys)
                    {
                        if (set.Contains(key.Path))
                        {
                            count++;
                            _userCache.Remove(key);
                        }
                    }
                }

                return count;
            }
        }

        private int ResetState(PersonalizationScope scope)
        {
            IDictionary cache = scope == PersonalizationScope.Shared ? _sharedCache : _userCache;

            var count = cache.Count;
            cache.Clear();
            return count;
        }

        public override int ResetUserState(string path, DateTime userInactiveSinceDate)
        {
            lock (_lock)
            {
                var count = 0;

                foreach (var (key, value) in _userCache)
                {
                    if (string.Equals(path, key.Path, StringComparison.OrdinalIgnoreCase) && value.LastAccessed.DateTime < userInactiveSinceDate)
                    {
                        count++;
                        _userCache.Remove(key);
                    }
                }

                return count;
            }
        }

        protected override void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob)
        {
            lock (_lock)
            {
                sharedDataBlob = _sharedCache.TryGetValue(path, out var sharedResult) ? sharedResult.Data : default;
                userDataBlob = _userCache.TryGetValue((path, userName), out var userResult) ? userResult.Data : default;
            }
        }

        protected override void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName)
        {
            lock (_lock)
            {
                _userCache.Remove((path, userName));
            }
        }

        protected override void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob)
        {
            lock (_lock)
            {
                _userCache[(path, userName)] = new(dataBlob, time.GetUtcNow());
            }
        }
    }
}
