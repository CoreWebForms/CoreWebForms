// MIT License.

using System.Collections;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

internal sealed class ExcludeObjBinDirectory : IFileProvider
{
    private readonly IFileProvider _provider;

    public ExcludeObjBinDirectory(IFileProvider provider)
    {
        _provider = provider;
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var contents = _provider.GetDirectoryContents(subpath);

        return string.IsNullOrEmpty(subpath) || subpath == "."
            ? new ExcludeDirectory(contents)
            : contents;
    }

    public IFileInfo GetFileInfo(string subpath) => _provider.GetFileInfo(subpath);

    public IChangeToken Watch(string filter) => _provider.Watch(filter);

    private sealed class ExcludeDirectory : IDirectoryContents
    {
        private readonly IDirectoryContents _contents;

        public ExcludeDirectory(IDirectoryContents contents)
        {
            _contents = contents;
        }

        public bool Exists => _contents.Exists;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var item in _contents)
            {
                if (string.Equals("bin", item.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("obj", item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
