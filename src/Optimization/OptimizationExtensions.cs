// MIT License.

using System.Collections;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

[assembly: TagPrefix("System.Web.Optimization", "webopt")]

namespace Microsoft.Extensions.DependencyInjection;

public static class OptimizationExtensions
{
    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder, Action<BundleCollection> configure)
    {
        builder.Services.AddTransient<IStartupFilter>(_ => new OptimizationStartup(configure));

        return builder.AddOptimization();
    }

    public static IWebFormsBuilder AddOptimization(this IWebFormsBuilder builder)
    {
        builder.Services.TryAddSingleton<VirtualPathProvider>(ctx => new Files(ctx.GetRequiredService<IWebHostEnvironment>().ContentRootFileProvider));

        return builder;
    }

    private sealed class OptimizationStartup(Action<BundleCollection> startup) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            => builder =>
            {
                startup(BundleTable.Bundles);

                next(builder);
            };
    }

    private sealed class Files(IFileProvider files) : VirtualPathProvider
    {
        private static string NormalizePath(string input) => VirtualPath.Create(input).Path;

        public override VirtualFile GetFile(string virtualPath) => new File(files, NormalizePath(virtualPath));

        public override VirtualDirectory GetDirectory(string virtualDir) => new Dir(files, NormalizePath(virtualDir));

        public override bool DirectoryExists(string virtualDir) => files.GetFileInfo(NormalizePath(virtualDir)) is { IsDirectory: true, Exists: true };

        public override bool FileExists(string virtualPath) => files.GetFileInfo(NormalizePath(virtualPath)) is { IsDirectory: false, Exists: true };

        private sealed class File(IFileProvider files, string path) : VirtualFile(path)
        {
            public override Stream Open() => files.GetFileInfo(VirtualPath).CreateReadStream();
        }

        private sealed class Dir(IFileProvider files, string path) : VirtualDirectory(path)
        {
            public override IEnumerable Directories => GetAll(false, true);

            public override IEnumerable Files => GetAll(true, false);

            public override IEnumerable Children => GetAll(true, true);

            private IEnumerable GetAll(bool returnFiles, bool returnDirectories)
            {
                foreach (var item in files.GetDirectoryContents(VirtualPath))
                {
                    if (returnFiles && !item.IsDirectory)
                    {
                        yield return new File(files, Path.Combine(VirtualPath, item.Name));
                    }

                    else if (returnDirectories && item.IsDirectory)
                    {
                        yield return new Dir(files, Path.Combine(VirtualPath, item.Name));
                    }
                }
            }
        }
    }
}
