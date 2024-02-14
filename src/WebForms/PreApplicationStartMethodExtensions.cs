// MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebForms;

namespace Microsoft.Extensions.DependencyInjection;

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
            var failOnError = options.Value.FailOnError;

            foreach (var startMethod in GetStartMethodAttributes(AssemblyLoadContext.Default))
            {
                if (!InvokeStartMethod(startMethod) && failOnError)
                {
                    throw new InvalidOperationException("Failed to run a requested PreApplicationStartMethodAttribute");
                }
            }
        }

        private IEnumerable<PreApplicationStartMethodAttribute> GetStartMethodAttributes(AssemblyLoadContext context)
        {
            var attributes = new List<PreApplicationStartMethodAttribute>();

            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
            {
                try
                {
                    using var stream = File.OpenRead(file);
                    using var re = new System.Reflection.PortableExecutable.PEReader(stream);

                    if (re.GetMetadataReader().HasAttribute(nameof(PreApplicationStartMethodAttribute), "System.Web"))
                    {
                        foreach (var attribute in context.LoadFromAssemblyPath(file).GetCustomAttributes<PreApplicationStartMethodAttribute>())
                        {
                            attributes.Add(attribute);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to open {Path} to check for PreApplicationStartMethodAttribute", file);
                }
            }

            return attributes;
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

                logger.LogInformation("Invoked PreApplicationStartMethodAttribute {Type}.{Name} in {Assembly}", startMethod.Type.FullName, startMethod.MethodName, startMethod.Type.Assembly.FullName);

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
