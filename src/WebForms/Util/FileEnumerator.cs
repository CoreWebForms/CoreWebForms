using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

internal abstract class FileData
{
    protected FileInfo _fileInfo;

    internal string Name
    {
        get { return _fileInfo.Name; }
    }

    internal string FullName
    {
        get { return _fileInfo.FullName; }
    }

    internal bool IsDirectory
    {
        get { return (_fileInfo.Attributes & FileAttributes.Directory) != 0; }
    }

    internal bool IsHidden
    {
        get { return (_fileInfo.Attributes & FileAttributes.Hidden) != 0; }
    }
}

internal class FileEnumerator : FileData, IEnumerable<FileData>, IEnumerator<FileData>, IDisposable
{
    private IEnumerator<FileInfo> _fileEnumerator;

    internal static FileEnumerator Create(string path)
    {
        return new FileEnumerator(path);
    }

    private FileEnumerator(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        _fileEnumerator = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).GetEnumerator();
    }

    public FileData Current => this;

    object IEnumerator.Current => this;

    public void Dispose()
    {
        _fileEnumerator.Dispose();
    }

    public bool MoveNext()
    {
        bool hasMoreFiles = _fileEnumerator.MoveNext();
        if (hasMoreFiles)
        {
            _fileInfo = _fileEnumerator.Current;
        }
        return hasMoreFiles;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public IEnumerator<FileData> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }
}
