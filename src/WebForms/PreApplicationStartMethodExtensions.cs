// MIT License.

using System.Reflection;
using System.Runtime.Loader;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

public static class PreApplicationStartMethodExtensions
{
    /// <summary>
    /// Enables support for assemblies marked with <see cref="PreApplicationStartMethodAttribute"/> to
    /// be hooked up to run on startup.
    /// </summary>
    public static ISystemWebAdapterBuilder AddPreApplicationStartMethod(this ISystemWebAdapterBuilder builder, bool failOnError = true)
    {
        builder.Services.AddOptions<PreApplicationOptions>()
            .Configure(options => options.FailOnError = failOnError);

        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, PreApplicationStartMethodStartupFilter>());
        return builder;
    }

    private sealed class PreApplicationOptions
    {
        public bool FailOnError { get; set; }
    }

    private sealed class PreApplicationStartMethodStartupFilter(IOptions<PreApplicationOptions> options, ILogger<PreApplicationStartMethodStartupFilter> logger) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                RunStartupMethods();
                next(builder);
            };

        private void RunStartupMethods()
        {
            if (Assembly.GetEntryAssembly() is not { } entry)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var failOnError = options.Value.FailOnError;

            ShimContext context = null;

            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*ScriptManager*.dll"))
            {
                context ??= new();

                if (context.LoadFromAssemblyPath(file) is { } assembly && assembly.GetCustomAttributes<PreApplicationStartMethodAttribute>() is { } startMethods)
                {
                    foreach (var startMethod in startMethods)
                    {
                        if (!InvokeStartMethod(startMethod) && failOnError)
                        {
                            throw new InvalidOperationException("Failed to run a requested PreApplicationStartMethodAttribute");
                        }
                    }
                }
            }
        }

        private sealed class ShimContext : AssemblyLoadContext
        {
            protected override Assembly Load(AssemblyName assemblyName)
            {
                if (assemblyName.Name == "System.Web")
                {
                    var a = LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, "System.Web.dll"));
                    return a;
                }

                return base.Load(assemblyName);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("System.Web"))
            {

            }

            return null;
        }

        private bool InvokeStartMethod(PreApplicationStartMethodAttribute startMethod)
        {
            var method = startMethod.Type.GetMethod(startMethod.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method is null)
            {
                logger.LogError("No method available {Name} for PreApplicationStartMethodAttribute in {Assembly}", startMethod.MethodName, startMethod.Type.Assembly);
                return false;
            }

            if (method.GetParameters().Length != 0 || method.ReturnType != typeof(void))
            {
                logger.LogError("Invalid method available {Name} for PreApplicationStartMethodAttribute in {Assembly}", startMethod.MethodName, startMethod.Type.Assembly);
                return false;
            }

            logger.LogInformation("Invoking PreApplicationStartMethodAttribute {Type}.{Name} in {Assembly}", startMethod.Type.FullName, startMethod.MethodName, startMethod.Type.Assembly.FullName);

            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    var instance = Activator.CreateInstance(startMethod.Type);
                    method.Invoke(instance, null);
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to run PreApplicationStartMethodAttribute {Type}.{Name} in {Assembly}", startMethod.Type.FullName, startMethod.MethodName, startMethod.Type.Assembly.FullName);
                return false;
            }
        }
    }
}
