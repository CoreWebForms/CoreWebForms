// MIT License.

using Microsoft.Extensions.FileProviders;

namespace WebForms.Compiler.Dynamic;

internal static class FileProviderExtensions
{
    public static IEnumerable<(IFileInfo File, string FullPath)> GetFiles(this IFileProvider provider)
    {
        var paths = new Queue<string>();
        paths.Enqueue(string.Empty);

        while (paths.Count > 0)
        {
            var subpath = paths.Dequeue();
            var directory = provider.GetDirectoryContents(subpath);

            foreach (var item in directory)
            {
                if (item.IsDirectory)
                {
                    paths.Enqueue(Path.Combine(subpath, item.Name));
                }
                else
                {
                    yield return (item, Path.Combine(subpath, item.Name));
                }
            }
        }
    }
}
